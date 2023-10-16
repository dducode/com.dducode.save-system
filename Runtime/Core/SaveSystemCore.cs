using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SaveSystem.CheckPoints;
using SaveSystem.Exceptions;
using SaveSystem.Handlers;
using SaveSystem.Internal;
using SaveSystem.UnityHandlers;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Logger = SaveSystem.Internal.Logger;
using Object = UnityEngine.Object;


namespace SaveSystem.Core {

    /// <summary>
    /// The Core of the Save System. It accepts <see cref="ObjectHandler{TO}">object handlers</see>
    /// and <see cref="IPersistentObject">persistent objects</see>
    /// and starts saving in three main modes - autosave, quick-save and save at checkpoint.
    /// Also it starts the saving when the player exit the game
    /// </summary>
    public static partial class SaveSystemCore {

        private static readonly string DestroyedCheckpointsPath =
            Path.Combine("save_system", "internal", "destroyed_checkpoints.bytes");

        private static readonly string LastSceneIndexPath =
            Path.Combine("save_system", "internal", "last_scene_index.bytes");

        private static SaveEvents m_enabledSaveEvents;
        private static float m_savePeriod;
        private static bool m_isParallel;
        private static bool m_debugEnabled;
        private static bool m_destroyCheckPoints;
        private static string m_playerTag;

        private static Action<SaveType> m_onSaveStart;
        private static Action<SaveType> m_onSaveEnd;

        /// It's used for delayed async saving
        private static readonly Queue<Func<UniTask>> SavingRequests = new();

        /// It will be canceled before exit game
        private static CancellationTokenSource m_cancellationSource;

        private static readonly ConcurrentBag<IObjectHandler> Handlers = new();
        private static readonly ConcurrentBag<IAsyncObjectHandler> AsyncHandlers = new();
        private static KeyCode m_quickSaveKey;

        private static List<Vector3> m_destroyedCheckpoints;
        private static int m_lastSceneIndex;

        private static bool m_autoSaveEnabled;
        private static float m_autoSaveLastTime;
        private static bool m_requestsQueueIsFree = true;
        private static IProgress<float> m_progress;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize () {
            var saveSystemLoop = new PlayerLoopSystem {
                type = typeof(SaveSystemCore),
                updateDelegate = UpdateSystem
            };

            SetPlayerLoop(PlayerLoop.GetCurrentPlayerLoop(), saveSystemLoop);
            SetSettings(Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings)));
            LoadInternalData();
            ResetOnExitPlayMode();

            m_cancellationSource = new CancellationTokenSource();
            Application.quitting += () => m_cancellationSource.Cancel();

            if (DebugEnabled)
                Logger.Log("Save System Core initialized");
        }


        private static void SetPlayerLoop (PlayerLoopSystem modifiedLoop, PlayerLoopSystem saveSystemLoop) {
            if (PlayerLoopManager.TryInsertSubSystem(ref modifiedLoop, saveSystemLoop, typeof(PreLateUpdate)))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError($"Failed insert system: {saveSystemLoop}");
        }


        private static void SetSettings ([NotNull] SaveSystemSettings settings) {
            if (settings == null) {
                throw new ArgumentNullException(
                    nameof(settings), "Save system settings not found. Did you delete, rename or transfer them?"
                );
            }

            // Core settings
            m_enabledSaveEvents = settings.enabledSaveEvents;
            SetupEvents(m_enabledSaveEvents);
            m_savePeriod = settings.savePeriod;
            m_isParallel = settings.isParallel;
            m_debugEnabled = settings.debugEnabled;

            // Checkpoints settings
            m_destroyCheckPoints = settings.destroyCheckPoints;
            m_playerTag = settings.playerTag;
        }


        private static void LoadInternalData () {
            using (UnityReader reader = UnityHandlersFactory.CreateDirectReader(DestroyedCheckpointsPath)) {
                m_destroyedCheckpoints = reader?.ReadVector3Array().ToList() ?? new List<Vector3>();
            }

            using (UnityReader reader = UnityHandlersFactory.CreateDirectReader(LastSceneIndexPath)) {
                m_lastSceneIndex = reader?.ReadInt() ?? -1;
            }
        }


        static partial void ResetOnExitPlayMode ();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Start () {
            FindAndInvokeProjectBootstrap();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private static void FindAndInvokeProjectBootstrap () {
            MethodInfo[] methods = (from behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                from methodInfo in behaviour.GetType()
                   .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                where methodInfo.GetParameters().Length == 0
                      && methodInfo.IsDefined(typeof(ProjectBootstrapAttribute))
                select methodInfo).ToArray();

            switch (methods.Length) {
                case 0:
                    return;
                case > 1:
                    throw new SaveSystemException(
                        $"More than one methods or objects implement {nameof(ProjectBootstrapAttribute)}, it's not supported"
                    );
            }

            MethodInfo method = methods.First();
            method.Invoke(Object.FindFirstObjectByType(method.DeclaringType), null);
        }


        internal static void SaveAtCheckpoint (CheckPointBase checkPoint, Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            checkPoint.Disable();

            const string message = "Successful save at checkpoint";
            ScheduleSave(SaveType.SaveAtCheckpoint, message, m_cancellationSource.Token);

            if (checkPoint == null)
                return;

            if (DestroyCheckPoints) {
                m_destroyedCheckpoints.Add(checkPoint.transform.position);
                checkPoint.Destroy();
                SaveDestroyedCheckpoints();
            }
            else
                checkPoint.Enable();
        }


        private static void SaveDestroyedCheckpoints () {
            using UnityWriter writer = UnityHandlersFactory.CreateDirectWriter(DestroyedCheckpointsPath);
            writer.Write(m_destroyedCheckpoints);
        }


        private static async void UpdateSystem () {
            if (m_cancellationSource.IsCancellationRequested)
                return;

            /*
             * Lock queue and call all requests only in one thread
             * This is necessary to prevent sharing of the same file
             */
            if (m_requestsQueueIsFree) {
                m_requestsQueueIsFree = false;

                while (SavingRequests.Count > 0) {
                    if (m_cancellationSource.IsCancellationRequested) {
                        m_requestsQueueIsFree = true;
                        SavingRequests.Clear();
                        return;
                    }

                    await SavingRequests.Dequeue().Invoke();
                }

                m_requestsQueueIsFree = true;
            }

            if (Input.GetKeyDown(m_quickSaveKey))
                QuickSave();

            if (m_autoSaveEnabled)
                AutoSave();
        }


        private static void AutoSave () {
            if (m_autoSaveLastTime + SavePeriod < Time.time) {
                const string message = "Successful auto save";
                ScheduleSave(SaveType.AutoSave, message, m_cancellationSource.Token);
                m_autoSaveLastTime = Time.time;
            }
        }


        private static void SetupEvents (SaveEvents enabledSaveEvents) {
            m_autoSaveEnabled = false;
            Application.quitting -= SaveBeforeExit;
            Application.focusChanged -= OnFocusLost;
            Application.lowMemory -= OnLowMemory;

            switch (enabledSaveEvents) {
                case SaveEvents.None:
                    break;
                case not SaveEvents.All:
                    m_autoSaveEnabled = enabledSaveEvents.HasFlag(SaveEvents.AutoSave);
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnExit))
                        Application.quitting += SaveBeforeExit;
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnFocusLost))
                        Application.focusChanged += OnFocusLost;
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnLowMemory))
                        Application.lowMemory += OnLowMemory;
                    break;
                case SaveEvents.All:
                    m_autoSaveEnabled = true;
                    Application.quitting += SaveBeforeExit;
                    Application.focusChanged += OnFocusLost;
                    Application.lowMemory += OnLowMemory;
                    break;
            }
        }


        private static void SaveBeforeExit () {
            const string message = "Successful saving during the quitting";
            ProcessSave(SaveType.OnExit, message);
        }


        private static void OnFocusLost (bool hasFocus) {
            if (!hasFocus) {
                const string message = "Successful saving on focus lost";
                ScheduleSave(SaveType.OnFocusLost, message, m_cancellationSource.Token);
            }
        }


        private static void OnLowMemory () {
            const string message = "Successful saving on low memory";
            ScheduleSave(SaveType.OnLowMemory, message, m_cancellationSource.Token);
        }


        private static void ScheduleSave (SaveType saveType, string debugMessage, CancellationToken token) {
            SavingRequests.Enqueue(async () => {
                m_onSaveStart?.Invoke(saveType);

                SaveHandlers();
                await SaveAsyncHandlers(token);
                if (token.IsCancellationRequested)
                    return;

                m_onSaveEnd?.Invoke(saveType);

                if (DebugEnabled)
                    Logger.Log(debugMessage);
            });
        }


        private static void ProcessSave (SaveType saveType, string debugMessage) {
            m_onSaveStart?.Invoke(saveType);

            SaveHandlers();
            Task.WaitAll(Task.Run(SaveAsyncHandlers));

            m_onSaveEnd?.Invoke(saveType);

            if (DebugEnabled)
                Logger.Log(debugMessage);
        }


        private static void SaveHandlers () {
            try {
                if (IsParallel) {
                    Parallel.ForEach(Handlers, objectHandler => objectHandler.Save());
                }
                else {
                    foreach (IObjectHandler objectHandler in Handlers)
                        objectHandler.Save();
                }
            }
            catch (Exception ex) {
                throw new SaveSystemException(
                    $"An internal exception was thrown, message: {ex.Message}",
                    ex
                );
            }
        }


        private static async UniTask SaveAsyncHandlers () {
            await SaveAsyncHandlers(default);
        }


        private static async UniTask SaveAsyncHandlers (CancellationToken token) {
            if (token.IsCancellationRequested)
                return;

            float completedTasks = 0;

            try {
                if (IsParallel) {
                    await ParallelLoop.ForEachAsync(
                        AsyncHandlers,
                        async asyncHandler => await asyncHandler.SaveAsync(token),
                        m_progress, m_cancellationSource.Token
                    );
                }
                else {
                    foreach (IAsyncObjectHandler asyncHandler in AsyncHandlers) {
                        await asyncHandler.SaveAsync(token);
                        if (token.IsCancellationRequested)
                            return;
                        m_progress?.Report(++completedTasks / AsyncHandlers.Count);
                    }
                }
            }
            catch (Exception ex) {
                throw new SaveSystemException(
                    $"An internal exception was thrown, message: {ex.Message}",
                    ex
                );
            }
        }


        private static void LoadHandlers () {
            try {
                if (IsParallel) {
                    Parallel.ForEach(Handlers, objectHandler => objectHandler.Load());
                }
                else {
                    foreach (IObjectHandler objectHandler in Handlers)
                        objectHandler.Load();
                }
            }
            catch (Exception ex) {
                throw new SaveSystemException(
                    $"An internal exception was thrown, message: {ex.Message}",
                    ex
                );
            }
        }


        private static async UniTask LoadAsyncHandlers () {
            try {
                if (IsParallel) {
                    await ParallelLoop.ForEachAsync(
                        AsyncHandlers, async asyncHandler => await asyncHandler.LoadAsync()
                    );
                }
                else {
                    foreach (IAsyncObjectHandler asyncHandler in AsyncHandlers)
                        await asyncHandler.LoadAsync();
                }
            }
            catch (Exception ex) {
                throw new SaveSystemException(
                    $"An internal exception was thrown, message: {ex.Message}",
                    ex
                );
            }
        }


        private static void OnSceneLoaded (Scene scene, LoadSceneMode loadMode) {
            Handlers.Clear();
            AsyncHandlers.Clear();
            m_lastSceneIndex = scene.buildIndex;
            SaveLastSceneIndex();
            DestroyTriggeredCheckpoints();
            FindAndInvokeSceneBootstrap();
        }


        private static void SaveLastSceneIndex () {
            using UnityWriter writer = UnityHandlersFactory.CreateDirectWriter(LastSceneIndexPath);
            writer.Write(m_lastSceneIndex);
        }


        private static void DestroyTriggeredCheckpoints () {
            IReadOnlyCollection<CheckPointBase> checkPoints =
                Object.FindObjectsByType<CheckPointBase>(FindObjectsSortMode.None);

            var destroyedCheckpointsCopy = new List<Vector3>(m_destroyedCheckpoints);

            foreach (CheckPointBase checkPoint in checkPoints) {
                for (var i = 0; i < destroyedCheckpointsCopy.Count; i++) {
                    if (checkPoint.transform.position == destroyedCheckpointsCopy[i]) {
                        checkPoint.Destroy();
                        destroyedCheckpointsCopy.RemoveAt(i);
                        break;
                    }
                }
            }
        }


        private static async void FindAndInvokeSceneBootstrap () {
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            MethodInfo[] methods = (from behaviour in behaviours
                from methodInfo in behaviour.GetType()
                   .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                where methodInfo.GetParameters().Length == 0
                      && methodInfo.IsDefined(typeof(SceneBootstrapAttribute))
                select methodInfo).ToArray();

            switch (methods.Length) {
                case 0:
                    return;
                case > 1:
                    throw new SaveSystemException(
                        $"More than one methods or objects implement {nameof(SceneBootstrapAttribute)}, it's not supported"
                    );
            }

            MethodInfo method = methods.First();
            var sceneBootstrap = method.GetCustomAttribute<SceneBootstrapAttribute>();
            method.Invoke(Object.FindFirstObjectByType(method.DeclaringType), null);

            if (sceneBootstrap.loadHandlers)
                await LoadAllHandlers();
            if (sceneBootstrap.invokeCallbacks)
                InvokeCallbacks();
        }


        private static async UniTask LoadAllHandlers () {
            LoadHandlers();
            await LoadAsyncHandlers();

            if (DebugEnabled)
                Logger.Log("All registered handlers was loaded");
        }


        private static void InvokeCallbacks () {
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (MonoBehaviour behaviour in behaviours) {
                IEnumerable<MethodInfo> callbacks = from methodInfo in behaviour.GetType()
                       .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    where methodInfo.GetParameters().Length == 0
                          && methodInfo.IsDefined(typeof(BootstrapCallbackAttribute), false)
                    select methodInfo;

                foreach (MethodInfo callback in callbacks)
                    callback.Invoke(behaviour, null);
            }
        }

    }

}