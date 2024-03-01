using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    /// The Core of the Save System. It accepts <see cref="DynamicObjectGroup{TDynamic}">object group</see>
    /// and starts saving in three main modes - autosave, quick-save and save at checkpoint.
    /// Also it starts the saving when the player exit the game
    /// </summary>
    public static partial class SaveSystemCore {

        internal const string SaveSystemFolder = "save_system_internal";

        private const string ProfileMetadataExtension = ".profilemetadata";
        private const string DefaultProfileName = "default_profile";

        private static string m_profilesFolderPath;
        private static string m_dataPath;

        private static SaveProfile m_selectedSaveProfile;
        private static SaveEvents m_enabledSaveEvents;
        private static float m_savePeriod;
        private static bool m_isParallel;
        private static string m_playerTag;

        private static Action<SaveType> m_onSaveStart;
        private static Action<SaveType> m_onSaveEnd;

        /// It will be canceled before exit game
        private static CancellationTokenSource m_exitCancellation;

        private static readonly List<IRuntimeSerializable> SerializableObjects = new();
        private static readonly List<IAsyncRuntimeSerializable> AsyncSerializableObjects = new();
        private static int ObjectsCount => SerializableObjects.Count + AsyncSerializableObjects.Count;

    #if ENABLE_LEGACY_INPUT_MANAGER
        private static KeyCode m_quickSaveKey;
    #endif

    #if ENABLE_INPUT_SYSTEM
        private static InputAction m_quickSaveAction;
    #endif

        private static bool m_autoSaveEnabled;
        private static float m_autoSaveLastTime;
        private static IProgress<float> m_saveProgress;
        private static IProgress<float> m_loadProgress;
        private static readonly SynchronizationPoint SynchronizationPoint = new();
        private static bool m_savedBeforeExit;
        private static bool m_loaded;
        private static bool m_registrationClosed;
        private static Action<SceneHandler> m_passDataAction;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize () {
            var saveSystemLoop = new PlayerLoopSystem {
                type = typeof(SaveSystemCore),
                updateDelegate = UpdateSystem
            };

            SetPlayerLoop(PlayerLoop.GetCurrentPlayerLoop(), saveSystemLoop);
            SetSettings(Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings)));
            SetInternalSavedPaths();
            SetOnExitPlayModeCallback();

            m_selectedSaveProfile = new SaveProfile {
                Name = DefaultProfileName, ProfileDataFolder = DefaultProfileName
            };
            m_exitCancellation = new CancellationTokenSource();
            Logger.Log("Save System Core initialized");
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Start () {
            SceneManager.sceneLoaded += OnSceneLoaded;
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
            m_dataPath = Storage.PrepareBeforeUsing(settings.dataPath, false);
            m_savePeriod = settings.savePeriod;
            m_isParallel = settings.isParallel;
            Logger.EnabledLogs = settings.enabledLogs;

            // Checkpoints settings
            m_playerTag = settings.playerTag;
        }


        private static void SetInternalSavedPaths () {
            m_profilesFolderPath = Storage.PrepareBeforeUsing(
                Path.Combine(SaveSystemFolder, "save_profiles"), true
            );
        }


        static partial void SetOnExitPlayModeCallback ();


        private static async void OnSceneLoaded (Scene scene, LoadSceneMode sceneMode) {
            try {
                GameObject target = scene.GetRootGameObjects()
                   .FirstOrDefault(gameObject => gameObject.CompareTag(Tags.SceneHandlerTag));
                if (target == null)
                    return;

                var sceneHandler = target.GetComponent<SceneHandler>();
                sceneHandler.OnPreLoad();
                await SynchronizationPoint.ExecuteTask(async () =>
                    await sceneHandler.sceneContext.LoadSceneDataAsync(m_selectedSaveProfile, m_exitCancellation.Token)
                );
                m_passDataAction?.Invoke(sceneHandler);
                sceneHandler.OnPostLoad();
                m_passDataAction = null;
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        internal static void SaveAtCheckpoint (Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            ScheduleSave(SaveType.SaveAtCheckpoint);
        }


        private static void UpdateSystem () {
            if (m_exitCancellation.IsCancellationRequested)
                return;

            /*
             * Call saving request only in the one state machine
             * This is necessary to prevent sharing of the same file
             */
            SynchronizationPoint.ExecuteScheduledTask(m_exitCancellation.Token);

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
            try {
                continuation(await SaveObjects(CancellationToken.None));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        private static async UniTask<HandlingResult> SaveObjects (CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                await SaveGlobalData(token);
                await m_selectedSaveProfile.SaveProfileDataAsync(token);

                SceneSerializationContext[] sceneLoaders =
                    Object.FindObjectsByType<SceneSerializationContext>(FindObjectsSortMode.None);

                if (sceneLoaders.Length > 0) {
                    if (IsParallel && sceneLoaders.Length > 1) {
                        await ParallelLoop.ForEachAsync(
                            sceneLoaders,
                            async sceneLoader => await sceneLoader.SaveSceneDataAsync(m_selectedSaveProfile, token)
                        );
                    }
                    else {
                        foreach (SceneSerializationContext sceneLoader in sceneLoaders)
                            await sceneLoader.SaveSceneDataAsync(m_selectedSaveProfile, token);
                    }
                }

                Logger.Log("All registered objects was saved");
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Loading operation was canceled");
                return HandlingResult.Canceled;
            }
        }


        private static async UniTask SaveGlobalData (CancellationToken token) {
            if (ObjectsCount == 0)
                return;
            if (!m_loaded)
                Logger.LogWarning("Start saving when objects not loaded");

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                using var writer = new BinaryWriter(new MemoryStream());
                var completedTasks = 0;

                foreach (IRuntimeSerializable obj in SerializableObjects) {
                    obj.Serialize(writer);
                    ReportProgress(ref completedTasks, ObjectsCount, m_saveProgress);
                }

                foreach (IAsyncRuntimeSerializable obj in AsyncSerializableObjects) {
                    await obj.Serialize(writer, token);
                    ReportProgress(ref completedTasks, ObjectsCount, m_saveProgress);
                }

                await writer.WriteDataToFileAsync(m_dataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Global data saving was canceled");
            }
        }


        private static async UniTask<HandlingResult> LoadGlobalData (CancellationToken token) {
            if (m_loaded) {
                Logger.LogWarning("All objects already loaded");
                return HandlingResult.Canceled;
            }

            if (ObjectsCount == 0) {
                Logger.LogError("Cannot start loading operation - the Core hasn't any objects for loading");
                return HandlingResult.Error;
            }

            if (!File.Exists(m_dataPath)) {
                m_registrationClosed = m_loaded = true;
                return HandlingResult.FileNotExists;
            }

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(m_dataPath, token);

                var completedTasks = 0;

                foreach (IRuntimeSerializable serializable in SerializableObjects) {
                    serializable.Deserialize(reader);
                    ReportProgress(ref completedTasks, ObjectsCount, m_loadProgress);
                }

                foreach (IAsyncRuntimeSerializable serializable in AsyncSerializableObjects) {
                    await serializable.Deserialize(reader, token);
                    ReportProgress(ref completedTasks, ObjectsCount, m_loadProgress);
                }

                Logger.Log("Global data was loaded");
                m_loaded = true;
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Global data loading was canceled");
                return HandlingResult.Canceled;
            }
        }


        private static void ReportProgress (ref int completedTasks, int tasksCount, IProgress<float> progress) {
            progress?.Report((float)++completedTasks / tasksCount);
        }

    }

}