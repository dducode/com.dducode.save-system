using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SaveSystem.CheckPoints;
using SaveSystem.Handlers;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Logger = SaveSystem.InternalServices.Logger;
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
        /// It's used to enable/disable autosave loop
        /// </summary>
        /// <value> If true, autosave loop will be enabled, otherwise false </value>
        public static bool AutoSaveEnabled { get; set; }


        /// <summary>
        /// It's used into autosave loop to determine saving frequency
        /// </summary>
        /// <value> Saving period in seconds </value>
        public static float SavePeriod { get; set; }

        /// <summary>
        /// You can choose 3 saving modes - simple mode, async saving and multithreading saving
        /// </summary>
        /// <seealso cref="SaveMode"/>
        public static SaveMode SaveMode { get; set; }

        /// <summary>
        /// Defines method to save simple objects (in the main thread or in the thread pool)
        /// </summary>
        /// <remarks> It's only used when async save mode is selected. Otherwise it'll be ignored </remarks>
        public static AsyncMode AsyncMode { get; set; }

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
        private static readonly List<IAsyncObjectHandler> AsyncHandlers = new();
        private static KeyCode m_quickSaveKey;

        private static List<Vector3> m_destroyedCheckpoints = new();
        private static float m_autoSaveLastTime;
        private static bool m_requestsQueueIsFree = true;


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

            if (DebugEnabled)
                Logger.Log("Initialized");
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void LoadInternalData () {
            using UnityReader reader = UnityHandlersProvider.GetReader(InternalDataPath);

            if (reader.ReadFileDataToBuffer()) {
                m_destroyedCheckpoints = reader.ReadVector3Array().ToList();

                DeleteTriggeredCheckpoints();

                if (DebugEnabled)
                    Logger.Log("Internal data was loaded");
            }
        }


        /// <summary>
        /// Registers a single object to automatic save, quick-save, save at checkpoit and at save on exit.
        /// </summary>
        /// <param name="obj"> Object that will be saved </param>
        /// <param name="filePath"> Path to object saving </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static void RegisterPersistentObject (
            [NotNull] IPersistentObject obj,
            [NotNull] string filePath,
            [CallerMemberName] string caller = null
        ) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            ObjectHandler<IPersistentObject> objectHandler = ObjectHandlersFactory.CreateHandler(filePath, obj, caller);
            if (!ObjectHandlersFactory.RegisterImmediately)
                RegisterObjectHandler(objectHandler);

            if (DebugEnabled)
                Logger.Log("Persistent object was register");
        }


        /// <inheritdoc cref="RegisterPersistentObject"/>
        public static void RegisterPersistentObjectAsync (
            [NotNull] IPersistentObjectAsync obj,
            [NotNull] string filePath,
            [CallerMemberName] string caller = ""
        ) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            AsyncObjectHandler<IPersistentObjectAsync> asyncObjectHandler =
                ObjectHandlersFactory.CreateAsyncHandler(filePath, obj, caller);
            if (!ObjectHandlersFactory.RegisterImmediately)
                RegisterAsyncObjectHandler(asyncObjectHandler);

            if (DebugEnabled)
                Logger.Log("Async persistent object was register");
        }


        /// <summary>
        /// Registers a handler to automatic save, quick-save, save at checkpoit and at save on exit
        /// </summary>
        public static void RegisterObjectHandler ([NotNull] IObjectHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Handlers.Add(handler);

            if (DebugEnabled)
                Logger.Log("Object handler was register");
        }


        /// <summary>
        /// Registers an async handler to automatic save, quick-save, save at checkpoit and at save on exit
        /// </summary>
        public static void RegisterAsyncObjectHandler ([NotNull] IAsyncObjectHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            AsyncHandlers.Add(handler);

            if (DebugEnabled)
                Logger.Log("Async object handler was register");
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <param name="autoSaveEnabled"> <see cref="AutoSaveEnabled"/> </param>
        /// <param name="debugEnabled"> <see cref="DebugEnabled"/> </param>
        /// <param name="destroyCheckPoints"> <see cref="DestroyCheckPoints"/> </param>
        /// <param name="saveMode"> <see cref="SaveMode"/> </param>
        /// <param name="playerTag"> <see cref="PlayerTag"/> </param>
        /// <param name="savePeriod"> <see cref="SavePeriod"/> </param>
        /// <param name="asyncMode"> <see cref="AsyncMode"/> </param>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureParameters (
            bool autoSaveEnabled,
            bool debugEnabled,
            bool destroyCheckPoints,
            SaveMode saveMode,
            string playerTag,
            float savePeriod = 0,
            AsyncMode asyncMode = AsyncMode.OnPlayerLoop
        ) {
            AutoSaveEnabled = autoSaveEnabled;
            DebugEnabled = debugEnabled;
            DestroyCheckPoints = destroyCheckPoints;
            SaveMode = saveMode;
            PlayerTag = playerTag;
            SavePeriod = savePeriod;
            AsyncMode = asyncMode;
        }


        /// <summary>
        /// It's recommended to call before exit game for guaranteed save
        /// </summary>
        /// <param name="exitImmediately"> If true, application will be closed after saving.
        /// Pass false if you want to do anything after this </param>
        /// <remarks>
        /// It resets some internal parameters and isn't recommended to be called except before quitting
        /// </remarks>
        public static async UniTask SaveBeforeExit (bool exitImmediately) {
            const string debugMessage = "Successful saving before quitting";
            m_cancellationSource.Cancel();
            AutoSaveEnabled = false;
            m_quickSaveKey = default;

            switch (SaveMode) {
                case SaveMode.Simple:
                    SaveAll(SaveType.OnExit, debugMessage);
                    break;
                case SaveMode.Async:
                    await SaveAllAsync(SaveType.OnExit, debugMessage);
                    break;
                case SaveMode.Parallel:
                    SaveAllParallel(SaveType.OnExit, debugMessage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (exitImmediately)
                Application.Quit();
        }


        /// <summary>
        /// You can call it when the player presses any key
        /// </summary>
        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        public static void QuickSave () {
            const string message = "Successful quick-save";

            switch (SaveMode) {
                case SaveMode.Simple:
                    SaveAll(SaveType.QuickSave, message);
                    break;
                case SaveMode.Async:
                    SavingRequests.Enqueue(async () =>
                        await SaveAllAsync(SaveType.QuickSave, message, m_cancellationSource.Token)
                    );
                    break;
                case SaveMode.Parallel:
                    SaveAllParallel(SaveType.QuickSave, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// Binds any key with quick save
        /// </summary>
        public static void BindKey (KeyCode keyCode) {
            m_quickSaveKey = keyCode;
        }


        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        internal static void SaveAtCheckpoint (CheckPointBase checkPoint, Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            checkPoint.Disable();

            const string message = "Successful save at checkpoint";

            switch (SaveMode) {
                case SaveMode.Simple:
                    SaveAll(SaveType.SaveAtCheckpoint, message);
                    break;
                case SaveMode.Async:
                    SavingRequests.Enqueue(async () =>
                        await SaveAllAsync(SaveType.SaveAtCheckpoint, message, m_cancellationSource.Token)
                    );
                    break;
                case SaveMode.Parallel:
                    SaveAllParallel(SaveType.SaveAtCheckpoint, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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
            if (Input.GetKeyDown(m_quickSaveKey))
                QuickSave();

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

            if (AutoSaveEnabled)
                AutoSave();
        }


        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        private static void AutoSave () {
            if (m_autoSaveLastTime + SavePeriod < Time.time) {
                const string message = "Successful auto save";

                switch (SaveMode) {
                    case SaveMode.Simple:
                        SaveAll(SaveType.AutoSave, message);
                        break;
                    case SaveMode.Async:
                        SavingRequests.Enqueue(async () =>
                            await SaveAllAsync(SaveType.AutoSave, message, m_cancellationSource.Token)
                        );
                        break;
                    case SaveMode.Parallel:
                        SaveAllParallel(SaveType.AutoSave, message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                m_autoSaveLastTime = Time.time;
            }
        }


        private static void SaveAll (SaveType saveType, string debugMessage) {
            OnSaveStart?.Invoke(saveType);

            foreach (IObjectHandler objectHandler in Handlers)
                objectHandler.Save();

            OnSaveEnd?.Invoke(saveType);

            if (DebugEnabled)
                Logger.Log(debugMessage);
        }


        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        private static async UniTask<HandlingResult> SaveAllAsync (
            SaveType saveType, string debugMessage, CancellationToken token = default
        ) {
            OnSaveStart?.Invoke(saveType);

            HandlingResult result;

            for (var i = 0; i < AsyncHandlers.Count; i++) {
                result = await AsyncHandlers[i].SaveAsync(token);
                if (result != HandlingResult.Success)
                    return result;
            }

            result = await Catcher.TryHandle(async () => {
                switch (AsyncMode) {
                    case AsyncMode.OnPlayerLoop:
                        foreach (IObjectHandler handler in Handlers) {
                            handler.Save();
                            await UniTask.NextFrame(token);
                        }

                        break;
                    case AsyncMode.OnThreadPool:
                        await UniTask.RunOnThreadPool(() => {
                            foreach (IObjectHandler handler in Handlers)
                                handler.Save();
                        }, cancellationToken: token);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            if (result != HandlingResult.Success)
                return result;

            OnSaveEnd?.Invoke(saveType);

            if (DebugEnabled)
                Logger.Log(debugMessage);

            return HandlingResult.Success;
        }


        private static void SaveAllParallel (SaveType saveType, string debugMessage) {
            OnSaveStart?.Invoke(saveType);

            Parallel.ForEach(Handlers, objectHandler => objectHandler.Save());

            OnSaveEnd?.Invoke(saveType);

            if (DebugEnabled)
                Logger.Log(debugMessage);
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
            AutoSaveEnabled = settings.autoSaveEnabled;
            SavePeriod = settings.savePeriod;
            SaveMode = settings.saveMode;
            AsyncMode = settings.asyncMode;
            DebugEnabled = settings.debugEnabled;

            // Checkpoints settings
            DestroyCheckPoints = settings.destroyCheckPoints;
            PlayerTag = settings.playerTag;
        }


        private static void SaveInternalData () {
            using UnityWriter writer = UnityHandlersProvider.GetWriter(InternalDataPath);
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


        static partial void ResetOnExitPlayMode (
            PlayerLoopSystem modifiedLoop,
            PlayerLoopSystem saveSystemLoop
        );

    }

}