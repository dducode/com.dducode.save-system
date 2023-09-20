using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.CheckPoints;
using SaveSystem.Handlers;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;
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

        private const string InternalDataPath = "destroyed_checkpoints.bytes";

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
        /// You can enable async saving if you have a lot of objects or they're large enough
        /// </summary>
        /// <value> If true, all objects and handlers will be saved async, otherwise - synchronously </value>
        public static bool AsyncSaveEnabled { get; set; }

        /// <summary>
        /// Enables logs
        /// </summary>
        /// <remarks>
        /// It configures only simple logs, other logs (warnings and errors) will be written to console anyway.
        /// </remarks>
        public static bool DebugEnabled { get; set; }

        /// <summary>
        /// Determines whether checkpoints will be destroyed
        /// </summary>
        /// <value> If true, triggered checkpoint will be deleted from scene after saving </value>
        public static bool DestroyCheckPoints { get; set; }

        /// <summary>
        /// It's used to filtering messages from triggered checkpoints
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

        private static readonly List<ObjectHandler> Handlers = new();
        private static readonly Queue<Func<UniTask>> SaveQueue = new();
        private static List<Vector3> m_destroyedCheckpoints = new();
        private static float m_lastTimeSaving;
        private static bool m_saveQueueIsFree = true;
        private static CancellationTokenSource m_cancellationSource;


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
            string dataPath = Storage.GetFullPath(InternalDataPath);
            using UnityReader reader = UnityHandlersProvider.GetReader(dataPath);

            if (reader != null) {
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

            Handlers.Add(ObjectHandlersFactory.Create(obj, filePath, caller));

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
        /// <param name="asyncSaveEnabled"> <see cref="AsyncSaveEnabled"/> </param>
        /// <param name="debugEnabled"> <see cref="DebugEnabled"/> </param>
        /// <param name="destroyCheckPoints"> <see cref="DestroyCheckPoints"/> </param>
        /// <param name="playerTag"> <see cref="PlayerTag"/> </param>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureParameters (
            bool autoSaveEnabled, float savePeriod, bool asyncSaveEnabled,
            bool debugEnabled, bool destroyCheckPoints, string playerTag
        ) {
            AutoSaveEnabled = autoSaveEnabled;
            SavePeriod = savePeriod;
            AsyncSaveEnabled = asyncSaveEnabled;
            DebugEnabled = debugEnabled;
            DestroyCheckPoints = destroyCheckPoints;
            PlayerTag = playerTag;
        }


        /// <summary>
        /// You can call it when the player presses any key
        /// </summary>
        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        public static void QuickSave () {
            SaveQueue.Enqueue(async () => {
                OnSaveStart?.Invoke(SaveType.QuickSave);

                if (AutoSaveEnabled)
                    await SaveAllAsync(m_cancellationSource.Token);
                else
                    SaveAll();

                if (m_cancellationSource.IsCancellationRequested)
                    return;

                OnSaveEnd?.Invoke(SaveType.QuickSave);

                if (DebugEnabled)
                    InternalLogger.Log("Successful quick-save");
            });
        }


        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        internal static void SaveAtCheckpoint (CheckPointBase checkPoint, Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            SaveQueue.Enqueue(async () => {
                OnSaveStart?.Invoke(SaveType.SaveAtCheckpoint);

                checkPoint.Disable();

                if (AsyncSaveEnabled)
                    await SaveAllAsync(m_cancellationSource.Token);
                else
                    SaveAll();

                if (m_cancellationSource.IsCancellationRequested)
                    return;

                if (DestroyCheckPoints) {
                    m_destroyedCheckpoints.Add(checkPoint.transform.position);
                    checkPoint.Destroy();
                    SaveInternalData();
                }
                else
                    checkPoint.Enable();

                OnSaveEnd?.Invoke(SaveType.SaveAtCheckpoint);

                if (DebugEnabled)
                    InternalLogger.Log("Successful save at checkpoint");
            });
        }


        private static async void UpdateSystem () {
            if (m_saveQueueIsFree) {
                m_saveQueueIsFree = false;

                while (SaveQueue.Count > 0) {
                    if (m_cancellationSource.IsCancellationRequested)
                        return;

                    await SaveQueue.Dequeue().Invoke();
                }

                m_saveQueueIsFree = true;
            }

            if (AutoSaveEnabled)
                AutoSave();
        }


        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        private static void AutoSave () {
            if (m_lastTimeSaving + SavePeriod < Time.time) {
                SaveQueue.Enqueue(async () => {
                    OnSaveStart?.Invoke(SaveType.AutoSave);

                    if (AsyncSaveEnabled)
                        await SaveAllAsync(m_cancellationSource.Token);
                    else
                        SaveAll();
                    
                    if (m_cancellationSource.IsCancellationRequested)
                        return;

                    OnSaveEnd?.Invoke(SaveType.AutoSave);

                    if (DebugEnabled)
                        InternalLogger.Log("Successful auto save");
                });

                m_lastTimeSaving = Time.time;
            }
        }


        private static void SaveAll () {
            foreach (ObjectHandler objectHandler in Handlers)
                objectHandler.Save();
        }


        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        private static async UniTask SaveAllAsync (CancellationToken token) {
            for (var i = 0; i < Handlers.Count; i++) {
                if (token.IsCancellationRequested)
                    return;
                await Handlers[i].SetCancellationToken(token).SaveAsync();
            }
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
            AsyncSaveEnabled = settings.asyncSaveEnabled;
            DebugEnabled = settings.debugEnabled;
            
            // Checkpoints settings
            DestroyCheckPoints = settings.destroyCheckPoints;
            PlayerTag = settings.playerTag;
        }


        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        private static void SetSavingBeforeQuitting () {
            Application.quitting += () => {
                m_cancellationSource.Cancel();

                OnSaveStart?.Invoke(SaveType.OnExit);

                SaveAll();

                OnSaveEnd?.Invoke(SaveType.OnExit);

                if (DebugEnabled)
                    InternalLogger.Log("Successful saving before application quitting");
            };
        }


        private static void SaveInternalData () {
            string dataPath = Storage.GetFullPath(InternalDataPath);
            using UnityWriter writer = UnityHandlersProvider.GetWriter(dataPath);
            writer.Write(m_destroyedCheckpoints);
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