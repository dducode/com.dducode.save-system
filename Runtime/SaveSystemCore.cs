using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using SaveSystem.CheckPoints;
using SaveSystem.Exceptions;
using SaveSystem.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Logger = SaveSystem.Internal.Logger;
using Object = UnityEngine.Object;

#if ENABLE_INPUT_SYSTEM
#endif

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAlias = Cysharp.Threading.Tasks.UniTask;
using TaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
using TaskResult = Cysharp.Threading.Tasks.UniTask<SaveSystem.HandlingResult>;

#else
using TaskAlias = System.Threading.Tasks.Task;
using TaskBool = System.Threading.Tasks.Task<bool>;
using TaskResult = System.Threading.Tasks.Task<SaveSystem.HandlingResult>;
#endif


namespace SaveSystem {

    /// <summary>
    /// The Core of the Save System. It accepts <see cref="DynamicObjectGroup{T}">object group</see>
    /// and starts saving in three main modes - autosave, quick-save and save at checkpoint.
    /// Also it starts the saving when the player exit the game
    /// </summary>
    public static partial class SaveSystemCore {

        private const string SaveProfileExtension = ".saveprofile";

        private static readonly string DefaultProfilePath =
            PathPreparing.PrepareBeforeUsing(Path.Combine("save_system_internal", "default_profile.bytes"));

        private static readonly string DestroyedCheckpointsPath =
            PathPreparing.PrepareBeforeUsing(Path.Combine("save_system_internal", "destroyed_checkpoints.bytes"));

        private static readonly string LastSceneIndexPath =
            PathPreparing.PrepareBeforeUsing(Path.Combine("save_system_internal", "last_scene_index.bytes"));

        private static SaveEvents m_enabledSaveEvents;
        private static float m_savePeriod;
        private static bool m_isParallel;
        private static bool m_destroyCheckPoints;
        private static string m_playerTag;

        private static Action<SaveType> m_onSaveStart;
        private static Action<SaveType> m_onSaveEnd;

        /// It will be canceled before exit game
        private static CancellationTokenSource m_exitCancellation;

        private static readonly ConcurrentBag<IRuntimeSerializable> SerializableObjects = new();
        private static SaveProfile m_currentProfile;

    #if ENABLE_LEGACY_INPUT_MANAGER
        private static KeyCode m_quickSaveKey;
    #endif

    #if ENABLE_INPUT_SYSTEM
        private static InputAction m_quickSaveAction;
    #endif

        private static List<Vector3> m_destroyedCheckpoints;
        private static int m_lastSceneIndex;

        private static bool m_autoSaveEnabled;
        private static float m_autoSaveLastTime;
        private static IProgress<float> m_saveProgress;
        private static IProgress<float> m_loadProgress;
        private static readonly SynchronizationPoint SynchronizationPoint = new();
        private static bool m_savedBeforeExit;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize () {
            var saveSystemLoop = new PlayerLoopSystem {
                type = typeof(SaveSystemCore),
                updateDelegate = UpdateSystem
            };

            SetPlayerLoop(PlayerLoop.GetCurrentPlayerLoop(), saveSystemLoop);
            SetSettings(Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings)));
            LoadInternalData();
            SetOnExitPlayModeCallback();

            m_exitCancellation = new CancellationTokenSource();
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
            SetupEvents(m_enabledSaveEvents = settings.enabledSaveEvents);
            m_savePeriod = settings.savePeriod;
            m_isParallel = settings.isParallel;
            Logger.EnabledLogs = settings.enabledLogs;

            // Checkpoints settings
            m_destroyCheckPoints = settings.destroyCheckPoints;
            m_playerTag = settings.playerTag;
        }


        private static void LoadInternalData () {
            if (File.Exists(DestroyedCheckpointsPath)) {
                using var reader = new BinaryReader(File.Open(DestroyedCheckpointsPath, FileMode.Open));
                m_destroyedCheckpoints = reader.ReadArray<Vector3>().ToList();
            }
            else {
                m_destroyedCheckpoints = new List<Vector3>();
            }

            if (File.Exists(LastSceneIndexPath)) {
                using var reader = new BinaryReader(File.Open(LastSceneIndexPath, FileMode.Open));
                m_lastSceneIndex = reader.Read<int>();
            }
            else {
                m_lastSceneIndex = -1;
            }
        }


        static partial void SetOnExitPlayModeCallback ();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Start () {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }


        internal static void SaveAtCheckpoint (CheckPointBase checkPoint, Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            checkPoint.Disable();

            ScheduleSave(SaveType.SaveAtCheckpoint, "Successful save at checkpoint");

            if (checkPoint == null)
                return;

            if (DestroyCheckPoints) {
                m_destroyedCheckpoints.Add(checkPoint.transform.position);
                checkPoint.Destroy();
                SaveDestroyedCheckpoints();
            }
            else {
                checkPoint.Enable();
            }
        }


        private static void SaveDestroyedCheckpoints () {
            using var writer = new BinaryWriter(File.Open(DestroyedCheckpointsPath, FileMode.OpenOrCreate));
            writer.Write(m_destroyedCheckpoints.ToArray());
        }


        private static async void UpdateSystem () {
            if (m_exitCancellation.IsCancellationRequested)
                return;

            /*
             * Call saving request only in the one state machine
             * This is necessary to prevent sharing of the same file
             */
            try {
                await SynchronizationPoint.ExecuteTask(m_exitCancellation.Token);
            }
            catch (OperationCanceledException) {
                Logger.Log("Internal operation was canceled");
            }
            catch (Exception ex) {
                throw new SaveSystemException(
                    $"An internal exception was thrown, message: {ex.Message}",
                    ex
                );
            }

        #if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(m_quickSaveKey))
                QuickSave();
        #endif

        #if ENABLE_INPUT_SYSTEM
            if (m_quickSaveAction != null && m_quickSaveAction.WasPerformedThisFrame())
                QuickSave();
        #endif

            if (m_autoSaveEnabled)
                AutoSave();
        }


        private static void QuickSave () {
            ScheduleSave(SaveType.QuickSave, "Successful quick-save");
        }


        private static void AutoSave () {
            if (m_autoSaveLastTime + SavePeriod < Time.time) {
                ScheduleSave(SaveType.AutoSave, "Successful auto save");
                m_autoSaveLastTime = Time.time;
            }
        }


        private static void SetupEvents (SaveEvents enabledSaveEvents) {
            m_autoSaveEnabled = false;
            Application.wantsToQuit -= SaveBeforeExit;
            Application.focusChanged -= OnFocusLost;
            Application.lowMemory -= OnLowMemory;

            switch (enabledSaveEvents) {
                case SaveEvents.None:
                    break;
                case not SaveEvents.All:
                    m_autoSaveEnabled = enabledSaveEvents.HasFlag(SaveEvents.AutoSave);
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnExit))
                        Application.wantsToQuit += SaveBeforeExit;
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnFocusLost))
                        Application.focusChanged += OnFocusLost;
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnLowMemory))
                        Application.lowMemory += OnLowMemory;
                    break;
                case SaveEvents.All:
                    m_autoSaveEnabled = true;
                    Application.wantsToQuit += SaveBeforeExit;
                    Application.focusChanged += OnFocusLost;
                    Application.lowMemory += OnLowMemory;
                    break;
            }
        }


        private static bool SaveBeforeExit () {
            if (m_savedBeforeExit)
                return true;

            m_exitCancellation.Cancel();

            if (Application.isEditor) {
                Logger.LogWarning("Saving before the quitting is not supported in the editor");
                return m_savedBeforeExit = true;
            }
            else {
                m_onSaveStart?.Invoke(SaveType.OnExit);
                SaveObjectGroups(OnSaveEnd);
                return false;
            }

            void OnSaveEnd () {
                m_onSaveEnd?.Invoke(SaveType.OnExit);
                m_savedBeforeExit = true;
                Logger.Log("Successful saving before the quitting");
                Application.Quit();
            }
        }


        private static void OnFocusLost (bool hasFocus) {
            if (!hasFocus) {
                const string message = "Successful saving on focus lost";
                ScheduleSave(SaveType.OnFocusLost, message);
            }
        }


        private static void OnLowMemory () {
            const string message = "Successful saving on low memory";
            ScheduleSave(SaveType.OnLowMemory, message);
        }


        private static void ScheduleSave (SaveType saveType, string debugMessage) {
            SynchronizationPoint.SetTask(async token => {
                m_onSaveStart?.Invoke(saveType);
                await SaveObjectGroups(token);
                m_onSaveEnd?.Invoke(saveType);
                Logger.Log(debugMessage);
            });
        }


        private static async void SaveObjectGroups (Action continuation) {
            await SaveObjectGroups(CancellationToken.None);
            continuation();
        }


        private static void SaveProfileMetadata () {
            using var writer = new BinaryWriter(
                File.Open($"{m_currentProfile.Name}{SaveProfileExtension}", FileMode.OpenOrCreate)
            );
            m_currentProfile.Serialize(writer);
        }


        private static HandlingResult LoadProfileMetadata () {
            var path = $"{m_currentProfile.Name}{SaveProfileExtension}";
            if (!File.Exists(path))
                return HandlingResult.FileNotExists;

            using var reader = new BinaryReader(File.Open(path, FileMode.Open));
            m_currentProfile ??= new SaveProfile();
            m_currentProfile.Deserialize(reader);
            return HandlingResult.Success;
        }


        private static async TaskAlias SaveObjectGroups (CancellationToken token) {
            token.ThrowIfCancellationRequested();

            using var writer = new BinaryWriter(new MemoryStream());

            foreach (IRuntimeSerializable serializable in SerializableObjects)
                serializable.Serialize(writer);

            await writer.WriteDataToFileAsync(DefaultProfilePath, token);
            Logger.Log("All registered objects was saved");
        }


        private static async TaskResult LoadObjectGroups (CancellationToken token) {
            if (!File.Exists(DefaultProfilePath))
                return HandlingResult.FileNotExists;

            try {
                token.ThrowIfCancellationRequested();

                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(DefaultProfilePath, token);

                foreach (IRuntimeSerializable objectGroup in SerializableObjects)
                    objectGroup.Deserialize(reader);

                Logger.Log("All registered objects was loaded");
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Loading operation was canceled");
                return HandlingResult.Canceled;
            }
        }


        private static void OnSceneLoaded (Scene scene, LoadSceneMode loadMode) {
            // SaveLastSceneIndex(m_lastSceneIndex = scene.buildIndex);
            DestroyTriggeredCheckpoints();
        }


        private static void OnSceneUnloaded (Scene scene) {
            ClearObjectGroups();
        }


        private static void SaveLastSceneIndex (int lastSceneIndex) {
            using var writer = new BinaryWriter(File.Open(LastSceneIndexPath, FileMode.OpenOrCreate));
            writer.Write(lastSceneIndex);
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


        private static void ClearObjectGroups () {
            var savedGroups = new List<IRuntimeSerializable>();

            foreach (IRuntimeSerializable objectGroup in SerializableObjects) {
                if (objectGroup.DontDestroyOnSceneUnload)
                    savedGroups.Add(objectGroup);
            }

            SerializableObjects.Clear();
            foreach (IRuntimeSerializable objectGroup in savedGroups)
                SerializableObjects.Add(objectGroup);
        }

    }

}