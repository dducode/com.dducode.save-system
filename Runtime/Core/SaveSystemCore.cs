using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.CheckPoints;
using SaveSystem.Handlers;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;


namespace SaveSystem.Core {

    /// <summary>
    /// The Core of the Save System. It accepts <see cref="ObjectHandler">object handlers</see>
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

        private static readonly List<ObjectHandler> Handlers = new();
        private static List<Vector3> m_destroyedCheckpoints = new();
        private static SavingJobHandle m_savingJobHandle;
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
            SetSavingBeforeQuitting();
            ResetOnExitPlayMode(modifiedLoop, saveSystemLoop);
            m_cancellationSource = new CancellationTokenSource();

            if (DebugEnabled)
                InternalLogger.Log("Initialized");
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void LoadInternalData () {
            using UnityReader reader = UnityHandlersProvider.GetReader(InternalDataPath);

            if (reader.ReadFileDataToBuffer()) {
                m_destroyedCheckpoints = reader.ReadVector3Array().ToList();

                DeleteTriggeredCheckpoints();

                if (DebugEnabled)
                    InternalLogger.Log("Internal data was loaded");
            }
        }


        /// <summary>
        /// Registers a single object to automatic save, quick-save, save at checkpoit and at save on exit.
        /// </summary>
        /// <param name="obj"> Object that will be saved </param>
        /// <param name="filePath"> Path to object saving </param>
        /// <param name="caller"> A method where the object handler was created </param>
        /// <remarks> Internally, it creates an object handler without configured parameters </remarks>
        public static void RegisterPersistentObject (
            [NotNull] IPersistentObject obj,
            [NotNull] string filePath,
            [CallerMemberName] string caller = ""
        ) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            ObjectHandler objectHandler = ObjectHandlersFactory.Create(obj, filePath, caller);
            if (!ObjectHandlersFactory.RegisterImmediately)
                RegisterObjectHandler(objectHandler);

            if (DebugEnabled)
                InternalLogger.Log("Persistent object was register");
        }


        /// <summary>
        /// Registers a handler to automatic save, quick-save, save at checkpoit and at save on exit
        /// </summary>
        /// <param name="handler"> The handler from which a save will be called </param>
        public static void RegisterObjectHandler ([NotNull] ObjectHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Handlers.Add(handler);

            if (DebugEnabled)
                InternalLogger.Log("Object handler was register");
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <param name="autoSaveEnabled"> <see cref="AutoSaveEnabled"/> </param>
        /// <param name="savePeriod"> <see cref="SavePeriod"/> </param>
        /// <param name="saveMode"> <see cref="SaveMode"/> </param>
        /// <param name="debugEnabled"> <see cref="DebugEnabled"/> </param>
        /// <param name="destroyCheckPoints"> <see cref="DestroyCheckPoints"/> </param>
        /// <param name="playerTag"> <see cref="PlayerTag"/> </param>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureParameters (bool autoSaveEnabled, float savePeriod,
            SaveMode saveMode, bool debugEnabled, bool destroyCheckPoints, string playerTag
        ) {
            AutoSaveEnabled = autoSaveEnabled;
            SavePeriod = savePeriod;
            SaveMode = saveMode;
            DebugEnabled = debugEnabled;
            DestroyCheckPoints = destroyCheckPoints;
            PlayerTag = playerTag;
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
                        await SaveAllAsync(m_cancellationSource.Token, SaveType.QuickSave, message)
                    );
                    break;
                case SaveMode.Parallel:
                    ScheduleSaveJob(SaveType.QuickSave, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                        await SaveAllAsync(m_cancellationSource.Token, SaveType.SaveAtCheckpoint, message)
                    );
                    break;
                case SaveMode.Parallel:
                    ScheduleSaveJob(SaveType.SaveAtCheckpoint, message);
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
            m_savingJobHandle.Complete();

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
                            await SaveAllAsync(m_cancellationSource.Token, SaveType.AutoSave, message)
                        );
                        break;
                    case SaveMode.Parallel:
                        ScheduleSaveJob(SaveType.AutoSave, message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                m_autoSaveLastTime = Time.time;
            }
        }


        private static void SaveAll (SaveType saveType, string debugMessage) {
            OnSaveStart?.Invoke(saveType);

            foreach (ObjectHandler objectHandler in Handlers)
                objectHandler.Save();

            OnSaveEnd?.Invoke(saveType);

            if (DebugEnabled)
                InternalLogger.Log(debugMessage);
        }


        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        private static async UniTask SaveAllAsync (CancellationToken token, SaveType saveType, string debugMessage) {
            OnSaveStart?.Invoke(saveType);

            for (var i = 0; i < Handlers.Count; i++) {
                if (token.IsCancellationRequested)
                    return;
                await Handlers[i].SetCancellationToken(token).SaveAsync();
            }

            OnSaveEnd?.Invoke(saveType);

            if (DebugEnabled)
                InternalLogger.Log(debugMessage);
        }


        private static void ScheduleSaveJob (SaveType saveType, string debugMessage) {
            var savingJob = new SavingJob();
            m_savingJobHandle = new SavingJobHandle(
                saveType, debugMessage, savingJob.Schedule(Handlers.Count, 1));
        }


        private static void SetPlayerLoop (PlayerLoopSystem modifiedLoop, PlayerLoopSystem saveSystemLoop) {
            if (ModifyUpdateSystem(ref modifiedLoop, saveSystemLoop, ModifyType.Insert))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                InternalLogger.LogError("Insert system failed");
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
            DebugEnabled = settings.debugEnabled;

            // Checkpoints settings
            DestroyCheckPoints = settings.destroyCheckPoints;
            PlayerTag = settings.playerTag;
        }


        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        private static void SetSavingBeforeQuitting () {
            Application.quitting += () => {
                m_cancellationSource.Cancel();
                SaveAll(SaveType.OnExit, "Successful saving before application quitting");
            };
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



        private struct SavingJobHandle {

            private readonly SaveType m_saveType;
            private readonly string m_debugMessage;
            private JobHandle m_jobHandle;

            private bool m_isCompleted;


            public SavingJobHandle (SaveType saveType, string debugMessage, JobHandle jobHandle) {
                m_saveType = saveType;
                m_debugMessage = debugMessage;
                m_jobHandle = jobHandle;

                m_isCompleted = false;
            }


            public void Complete () {
                if (m_isCompleted)
                    return;

                m_jobHandle.Complete();
                OnSaveEnd?.Invoke(m_saveType);
                m_isCompleted = true;

                if (DebugEnabled)
                    InternalLogger.Log(m_debugMessage);
            }

        }



        private struct SavingJob : IJobParallelFor {

            public void Execute (int index) {
                Handlers[index].Save();
            }

        }

    }

}