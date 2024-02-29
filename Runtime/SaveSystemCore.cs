using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.CheckPoints;
using SaveSystem.Exceptions;
using SaveSystem.Internal;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Logger = SaveSystem.Internal.Logger;
using Object = UnityEngine.Object;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SaveSystem {

    /// <summary>
    /// The Core of the Save System. It accepts <see cref="DynamicObjectFactory{TDynamic}">object group</see>
    /// and starts saving in three main modes - autosave, quick-save and save at checkpoint.
    /// Also it starts the saving when the player exit the game
    /// </summary>
    public static partial class SaveSystemCore {

        private const string ProfileExtension = ".saveprofile";
        private const string DefaultProfileName = "default_profile";
        private const string SaveSystemExtension = ".savesystem";

        private static string m_profilesFolderPath;
        private static string m_destroyedCheckpointsPath;

        private static SaveProfile m_selectedSaveProfile;
        private static SaveEvents m_enabledSaveEvents;
        private static float m_savePeriod;
        private static bool m_isParallel;
        private static bool m_destroyCheckPoints;
        private static string m_playerTag;

        private static Action<SaveType> m_onSaveStart;
        private static Action<SaveType> m_onSaveEnd;

        /// It will be canceled before exit game
        private static CancellationTokenSource m_exitCancellation;

        private static readonly List<IRuntimeSerializable> SerializableObjects = new();

    #if ENABLE_LEGACY_INPUT_MANAGER
        private static KeyCode m_quickSaveKey;
    #endif

    #if ENABLE_INPUT_SYSTEM
        private static InputAction m_quickSaveAction;
    #endif

        private static List<Vector3> m_destroyedCheckpoints;

        private static bool m_allowSceneSaving;
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
            SetInternalSavedPaths();
            LoadInternalData();
            SetOnExitPlayModeCallback();

            m_selectedSaveProfile = new SaveProfile {
                Name = DefaultProfileName, DataPath = DefaultProfileName
            };
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
            m_allowSceneSaving = settings.allowSceneSaving;
            m_savePeriod = settings.savePeriod;
            m_isParallel = settings.isParallel;
            Logger.EnabledLogs = settings.enabledLogs;

            // Checkpoints settings
            m_destroyCheckPoints = settings.destroyCheckPoints;
            m_playerTag = settings.playerTag;
        }


        private static void SetInternalSavedPaths () {
            m_profilesFolderPath = Storage.PrepareBeforeUsing(
                Path.Combine("save_system_internal", "save_profiles"), true
            );
            m_destroyedCheckpointsPath = Storage.PrepareBeforeUsing(
                Path.Combine("save_system_internal", $"destroyed_checkpoints{SaveSystemExtension}"), false
            );
        }


        private static void LoadInternalData () {
            if (File.Exists(m_destroyedCheckpointsPath)) {
                using var reader = new BinaryReader(File.Open(m_destroyedCheckpointsPath, FileMode.Open));
                m_destroyedCheckpoints = reader.ReadArray<Vector3>().ToList();
            }
            else {
                m_destroyedCheckpoints = new List<Vector3>();
            }
        }


        static partial void SetOnExitPlayModeCallback ();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Start () {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        internal static void SaveAtCheckpoint (CheckPointBase checkPoint, Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            checkPoint.Disable();

            ScheduleSave(SaveType.SaveAtCheckpoint);

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
            using var writer = new BinaryWriter(File.Open(m_destroyedCheckpointsPath, FileMode.OpenOrCreate));
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
                await SynchronizationPoint.ExecuteScheduledTask(m_exitCancellation.Token);
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
            ScheduleSave(SaveType.QuickSave);
        }


        private static void AutoSave () {
            if (m_autoSaveLastTime + SavePeriod < Time.time) {
                ScheduleSave(SaveType.AutoSave);
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
                SaveObjects(OnSaveEnd);
                return false;
            }

            void OnSaveEnd (HandlingResult result) {
                m_onSaveEnd?.Invoke(SaveType.OnExit);
                m_savedBeforeExit = true;
                if (result is HandlingResult.Success)
                    Logger.Log("Successful saving before the quitting");
                else if (result is HandlingResult.Canceled)
                    Logger.LogWarning("The saving before the quitting was canceled");
                else if (result is HandlingResult.Error)
                    Logger.LogError("Some error was occured");
                Application.Quit();
            }
        }


        private static void OnFocusLost (bool hasFocus) {
            if (!hasFocus)
                ScheduleSave(SaveType.OnFocusLost);
        }


        private static void OnLowMemory () {
            ScheduleSave(SaveType.OnLowMemory);
        }


        private static void ScheduleSave (SaveType saveType) {
            SynchronizationPoint.ScheduleTask(async token => {
                m_onSaveStart?.Invoke(saveType);
                HandlingResult result = await SaveObjects(token);
                m_onSaveEnd?.Invoke(saveType);

                if (result is HandlingResult.Success)
                    Logger.Log($"{saveType}: success");
                else if (result is HandlingResult.Canceled)
                    Logger.LogWarning($"{saveType}: canceled");
                else if (result is HandlingResult.Error)
                    Logger.LogError($"{saveType}: error");

                return result;
            });
        }


        private static async void SaveObjects (Action<HandlingResult> continuation) {
            continuation(await SaveObjects(CancellationToken.None));
        }


        private static async UniTask<HandlingResult> SaveObjects (CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();

                using var writer = new BinaryWriter(new MemoryStream());
                SerializableObjects.RemoveAll(serializable => serializable.Equals(null));
                foreach (IRuntimeSerializable serializable in SerializableObjects)
                    serializable.Serialize(writer);

                if (!m_allowSceneSaving) {
                    Logger.Log("All registered objects was saved");
                    return HandlingResult.Success;
                }

                var sceneSerialization = Object.FindAnyObjectByType<SceneSerialization>();
                if (sceneSerialization == null)
                    throw new InvalidOperationException("No scene has a scene serialization object");
                writer.Write(sceneSerialization.sceneIndex);
                sceneSerialization.Serialize(writer);

                await writer.WriteDataToFileAsync(m_selectedSaveProfile.DataPath, token);
                Logger.Log("All registered objects was saved");
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Loading operation was canceled");
                return HandlingResult.Canceled;
            }
        }


        private static void OnSceneLoaded (Scene scene, LoadSceneMode loadMode) {
            DestroyTriggeredCheckpoints();
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

    }

}