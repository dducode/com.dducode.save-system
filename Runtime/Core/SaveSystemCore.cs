using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SaveSystem.CheckPoints;
using SaveSystem.Handlers;
using SaveSystem.Internal;
using SaveSystem.UnityHandlers;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
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

        private static readonly string InternalDataPath =
            Path.Combine("save_system", "internal", "destroyed_checkpoints.bytes");


        /// <summary>
        /// It's used to manage autosave loop, save on focus changed, on low memory and on quitting the game
        /// </summary>
        /// <seealso cref="SaveEvents"/>
        public static SaveEvents EnabledSaveEvents { get; set; }

        /// <summary>
        /// It's used into autosave loop to determine saving frequency
        /// </summary>
        /// <value> Saving period in seconds </value>
        /// <remarks> If it equals 0, saving will be executed at every frame </remarks>
        public static float SavePeriod { get; set; }

        /// <summary>
        /// Configure it to set parallel saving handlers
        /// </summary>
        public static bool IsParallel { get; set; }

        /// <summary>
        /// Enables logs
        /// </summary>
        /// <remarks>
        /// It configures only simple logs, other logs (warnings and errors) will be written to console anyway.
        /// </remarks>
        public static bool DebugEnabled { get; set; }

        /// <summary>
        /// Determines whether checkpoints will be destroyed after saving
        /// </summary>
        /// <value> If true, triggered checkpoint will be deleted from scene after saving </value>
        public static bool DestroyCheckPoints { get; set; }

        /// <summary>
        /// Player tag is used to filtering messages from triggered checkpoints
        /// </summary>
        /// <value> Tag of the player object </value>
        public static string PlayerTag { get; set; }

        /// <summary>
        /// Event that called before saving. It can be useful when you use async saving
        /// </summary>
        /// <value>
        /// Listeners that will be called when core will start saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveStart;

        /// <summary>
        /// Event that called after saving
        /// </summary>
        /// <value>
        /// Listeners that will be called when core will finish saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveEnd;

        /// It's used for delayed async saving
        private static readonly Queue<Func<UniTask>> SavingRequests = new();

        /// It will be canceled before exit game
        private static CancellationTokenSource m_cancellationSource;

        private static readonly ConcurrentBag<IObjectHandler> Handlers = new();
        private static readonly ConcurrentBag<IAsyncObjectHandler> AsyncHandlers = new();
        private static KeyCode m_quickSaveKey;

        private static List<Vector3> m_destroyedCheckpoints = new();
        private static bool m_autoSaveEnabled;
        private static float m_autoSaveLastTime;
        private static bool m_requestsQueueIsFree = true;
        private static IProgress<float> m_progress;
        private static bool m_exitSavingCompleted;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        private static void Initialize () {
            PlayerLoopSystem modifiedLoop = PlayerLoop.GetCurrentPlayerLoop();
            var saveSystemLoop = new PlayerLoopSystem {
                type = typeof(SaveSystemCore),
                updateDelegate = UpdateSystem
            };

            SetPlayerLoop(modifiedLoop, saveSystemLoop);
            SetSettings(Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings)));
            ResetOnExitPlayMode(modifiedLoop, saveSystemLoop);
            m_cancellationSource = new CancellationTokenSource();

            if (EnabledSaveEvents.HasFlag(SaveEvents.OnFocusChanged))
                Application.focusChanged += OnFocusChanged;

            if (EnabledSaveEvents.HasFlag(SaveEvents.OnLowMemory))
                Application.lowMemory += OnLowMemory;

            if (EnabledSaveEvents.HasFlag(SaveEvents.OnExit))
                Application.quitting += SaveBeforeExit;

            Application.quitting += () => m_cancellationSource.Cancel();

            if (DebugEnabled)
                Logger.Log("Initialized");
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void LoadInternalData () {
            using UnityReader reader = UnityHandlersFactory.CreateReader(InternalDataPath);

            if (reader.ReadFileDataToBuffer()) {
                m_destroyedCheckpoints = reader.ReadVector3Array().ToList();
                DeleteTriggeredCheckpoints();
            }
        }


        /// <summary>
        /// Registers a handler to automatic save, quick-save, save at checkpoit and at save on exit
        /// </summary>
        public static void RegisterObjectHandler ([NotNull] IObjectHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Handlers.Add(handler);

            if (DebugEnabled)
                Logger.Log($"{handler.GetType().Name} was register");
        }


        /// <summary>
        /// Registers an async handler to automatic save, quick-save, save at checkpoit and at save on exit
        /// </summary>
        public static void RegisterAsyncObjectHandler ([NotNull] IAsyncObjectHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            AsyncHandlers.Add(handler);

            if (DebugEnabled)
                Logger.Log($"{handler.GetType().Name} was register");
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <param name="enabledSaveEvents"></param>
        /// <param name="isParallel"></param>
        /// <param name="debugEnabled"> <see cref="DebugEnabled"/> </param>
        /// <param name="destroyCheckPoints"> <see cref="DestroyCheckPoints"/> </param>
        /// <param name="playerTag"> <see cref="PlayerTag"/> </param>
        /// <param name="savePeriod"> <see cref="SavePeriod"/> </param>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureParameters (
            SaveEvents enabledSaveEvents,
            bool isParallel,
            bool debugEnabled,
            bool destroyCheckPoints,
            string playerTag,
            float savePeriod = 0
        ) {
            EnabledSaveEvents = enabledSaveEvents;
            m_autoSaveEnabled = EnabledSaveEvents.HasFlag(SaveEvents.AutoSave);
            IsParallel = isParallel;
            DebugEnabled = debugEnabled;
            DestroyCheckPoints = destroyCheckPoints;
            PlayerTag = playerTag;
            SavePeriod = savePeriod;

            Application.focusChanged -= OnFocusChanged;
            Application.lowMemory -= OnLowMemory;
            Application.quitting -= SaveBeforeExit;

            if (EnabledSaveEvents.HasFlag(SaveEvents.OnFocusChanged))
                Application.focusChanged += OnFocusChanged;

            if (EnabledSaveEvents.HasFlag(SaveEvents.OnLowMemory))
                Application.lowMemory += OnLowMemory;

            if (EnabledSaveEvents.HasFlag(SaveEvents.OnExit))
                Application.quitting += SaveBeforeExit;

            if (DebugEnabled) {
                Logger.Log("Parameters was configured:" +
                           $"\nEnabled Save Events: {EnabledSaveEvents}" +
                           $"\nIs Parallel: {IsParallel}" +
                           $"\nDebug Enabled: {DebugEnabled}" +
                           $"\nDestroy Check Points: {DestroyCheckPoints}" +
                           $"\nPlayer Tag: {PlayerTag}" +
                           $"\nSave Period: {SavePeriod}"
                );
            }
        }


        /// <summary>
        /// Pass <see cref="IProgress{T}"> IProgress object </see> to observe async saving progress
        /// when it'll be started
        /// </summary>
        /// <remarks> The Core will report progress only during async save </remarks>
        public static void ObserveProgress ([NotNull] IProgress<float> progress) {
            m_progress = progress ?? throw new ArgumentNullException(nameof(progress));

            if (DebugEnabled)
                Logger.Log($"Progress observer {m_progress} was register");
        }


        /// <summary>
        /// Binds any key with quick save
        /// </summary>
        public static void BindKey (KeyCode keyCode) {
            m_quickSaveKey = keyCode;

            if (DebugEnabled)
                Logger.Log($"Key {m_quickSaveKey} was bind with quick save");
        }


        /// <summary>
        /// Call this to manually save handlers before quitting the application
        /// </summary>
        /// <remarks>
        /// This will immediately exit the game after saving.
        /// You should make sure that you don't need to do anything else before calling it
        /// </remarks>
        public static async UniTask SaveAndQuit () {
            m_cancellationSource.Cancel();
            m_autoSaveEnabled = false;
            m_quickSaveKey = default;

            OnSaveStart?.Invoke(SaveType.OnExit);
            SaveHandlers();
            await SaveAsyncHandlers();
            OnSaveEnd?.Invoke(SaveType.OnExit);

            if (DebugEnabled)
                Logger.Log("Successful async saving before the quitting");

            Application.quitting -= SaveBeforeExit;

        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
        }


        /// <summary>
        /// You can call it when any event was happened
        /// </summary>
        public static void QuickSave () {
            const string message = "Successful quick-save";
            ProcessSave(SaveType.QuickSave, message, m_cancellationSource.Token);
        }


        internal static void SaveAtCheckpoint (CheckPointBase checkPoint, Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            checkPoint.Disable();

            const string message = "Successful save at checkpoint";
            ProcessSave(SaveType.SaveAtCheckpoint, message, m_cancellationSource.Token);

            if (checkPoint == null)
                return;

            if (DestroyCheckPoints) {
                m_destroyedCheckpoints.Add(checkPoint.transform.position);
                checkPoint.Destroy();
                SaveInternalData();
            }
            else
                checkPoint.Enable();
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
                ProcessSave(SaveType.AutoSave, message, m_cancellationSource.Token);
                m_autoSaveLastTime = Time.time;
            }
        }


        private static void OnFocusChanged (bool hasFocus) {
            if (!hasFocus) {
                const string message = "Successful saving on focus changed";
                ProcessSave(SaveType.OnFocusChanged, message, m_cancellationSource.Token);
            }
        }


        private static void OnLowMemory () {
            const string message = "Successful saving on low memory";
            ProcessSave(SaveType.OnLowMemory, message, m_cancellationSource.Token);
        }


        private static void SaveBeforeExit () {
            const string message = "Successful saving during the quitting";
            OnSaveStart?.Invoke(SaveType.OnExit);

            SaveHandlers();
            Task.WaitAll(Task.Run(SaveAsyncHandlers));

            OnSaveEnd?.Invoke(SaveType.OnExit);

            if (DebugEnabled)
                Logger.Log(message);
        }


        private static void ProcessSave (SaveType saveType, string debugMessage, CancellationToken token) {
            SavingRequests.Enqueue(async () => {
                OnSaveStart?.Invoke(saveType);

                SaveHandlers();
                await SaveAsyncHandlers(token);
                if (token.IsCancellationRequested)
                    return;

                OnSaveEnd?.Invoke(saveType);

                if (DebugEnabled)
                    Logger.Log(debugMessage);
            });
        }


        private static void SaveHandlers () {
            if (IsParallel) {
                Parallel.ForEach(Handlers, objectHandler => objectHandler.Save());
            }
            else {
                foreach (IObjectHandler objectHandler in Handlers)
                    objectHandler.Save();
            }
        }


        private static async UniTask SaveAsyncHandlers () {
            await SaveAsyncHandlers(default);
        }


        private static async UniTask SaveAsyncHandlers (CancellationToken token) {
            if (token.IsCancellationRequested)
                return;

            float completedTasks = 0;

            if (IsParallel) {
                await ParallelLoop.ForEachAsync(AsyncHandlers, async asyncHandler => {
                    await asyncHandler.SaveAsync(token);
                    if (token.IsCancellationRequested)
                        return;

                    completedTasks++;
                    m_progress?.Report(completedTasks / AsyncHandlers.Count);
                });
            }
            else {
                foreach (IAsyncObjectHandler asyncHandler in AsyncHandlers) {
                    await asyncHandler.SaveAsync(token);
                    if (token.IsCancellationRequested)
                        return;

                    completedTasks++;
                    m_progress?.Report(completedTasks / AsyncHandlers.Count);
                }
            }
        }


        private static void SetPlayerLoop (PlayerLoopSystem modifiedLoop, PlayerLoopSystem saveSystemLoop) {
            if (ModifyUpdateSystem(ref modifiedLoop, saveSystemLoop, ModifyType.Insert))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError("Insert system failed");
        }


        private static void SetSettings ([NotNull] SaveSystemSettings settings) {
            if (settings == null) {
                throw new ArgumentNullException(
                    nameof(settings), "Save system settings not found. Did you delete, rename or transfer them?"
                );
            }

            // Core settings
            EnabledSaveEvents = settings.enabledSaveEvents;
            m_autoSaveEnabled = (EnabledSaveEvents & SaveEvents.AutoSave) != 0;
            SavePeriod = settings.savePeriod;
            IsParallel = settings.isParallel;
            DebugEnabled = settings.debugEnabled;

            // Checkpoints settings
            DestroyCheckPoints = settings.destroyCheckPoints;
            PlayerTag = settings.playerTag;
        }


        private static void SaveInternalData () {
            using UnityWriter writer = UnityHandlersFactory.CreateWriter(InternalDataPath);
            writer.Write(m_destroyedCheckpoints);
            writer.WriteBufferToFile();
        }


        private static bool ModifyUpdateSystem (
            ref PlayerLoopSystem currentPlayerLoop,
            PlayerLoopSystem insertedSubSystem,
            ModifyType modifyType
        ) {
            if (currentPlayerLoop.type == typeof(PreLateUpdate)) {
                switch (modifyType) {
                    case ModifyType.Insert:
                        InsertSubSystemAtLast(ref currentPlayerLoop, insertedSubSystem);
                        break;
                    case ModifyType.Remove:
                        RemoveSubSystem(ref currentPlayerLoop);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(modifyType), modifyType, null);
                }

                return true;
            }

            if (currentPlayerLoop.subSystemList != null)
                for (var i = 0; i < currentPlayerLoop.subSystemList.Length; i++)
                    if (ModifyUpdateSystem(ref currentPlayerLoop.subSystemList[i], insertedSubSystem, modifyType))
                        return true;

            return false;
        }


        private static void InsertSubSystemAtLast (ref PlayerLoopSystem currentPlayerLoop,
            PlayerLoopSystem insertedSubSystem
        ) {
            var newSubSystems = new PlayerLoopSystem[currentPlayerLoop.subSystemList.Length + 1];

            for (var i = 0; i < currentPlayerLoop.subSystemList.Length; i++)
                newSubSystems[i] = currentPlayerLoop.subSystemList[i];

            newSubSystems[^1] = insertedSubSystem;
            currentPlayerLoop.subSystemList = newSubSystems;
        }


        private static void RemoveSubSystem (ref PlayerLoopSystem currentPlayerLoop) {
            var newSubSystems = new PlayerLoopSystem[currentPlayerLoop.subSystemList.Length - 1];

            var j = 0;

            foreach (PlayerLoopSystem subSystem in currentPlayerLoop.subSystemList) {
                if (subSystem.type == typeof(SaveSystemCore))
                    continue;

                newSubSystems[j] = subSystem;
                j++;
            }

            currentPlayerLoop.subSystemList = newSubSystems;
        }


        private static void DeleteTriggeredCheckpoints () {
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


        static partial void ResetOnExitPlayMode (PlayerLoopSystem modifiedLoop, PlayerLoopSystem saveSystemLoop);

    }

}