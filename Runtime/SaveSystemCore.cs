using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
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


        internal static void SaveAtCheckpoint (Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            ScheduleSave(SaveType.SaveAtCheckpoint);
        }


        internal static async UniTask<HandlingResult> LoadProfileDataAsync (
            SaveProfile profile, CancellationToken token
        ) {
            string dataPath = Path.Combine(profile.DataPath, $"{profile.Name}.profiledata");
            if (!File.Exists(dataPath))
                return HandlingResult.FileNotExists;

            try {
                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(dataPath, token);
                await profile.DeserializeScope(reader);

                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        internal static async UniTask<HandlingResult> LoadSceneDataAsync (
            SceneLoader sceneLoader, SaveProfile profile, CancellationToken token
        ) {
            string dataPath = Path.Combine(profile.DataPath, $"{sceneLoader.sceneName}.scenedata");
            if (!File.Exists(dataPath))
                return HandlingResult.FileNotExists;

            using var reader = new BinaryReader(new MemoryStream());
            await reader.ReadDataFromFileAsync(dataPath, token);
            await sceneLoader.DeserializeScope(reader);

            return HandlingResult.Success;
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
            continuation(await SaveObjects(CancellationToken.None));
        }


        private static async UniTask<HandlingResult> SaveObjects (CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                if (ObjectsCount > 0)
                    await SaveGlobalData(token);
                if (m_selectedSaveProfile.ObjectsCount > 0)
                    await SaveProfileData(m_selectedSaveProfile, token);

                SceneLoader[] sceneLoaders =
                    Object.FindObjectsByType<SceneLoader>(FindObjectsSortMode.None);

                if (sceneLoaders.Length > 0) {
                    if (IsParallel) {
                        await ParallelLoop.ForEachAsync(
                            sceneLoaders,
                            async sceneLoader => await SaveSceneData(sceneLoader, m_selectedSaveProfile, token)
                        );
                    }
                    else {
                        foreach (SceneLoader sceneLoader in sceneLoaders)
                            await SaveSceneData(sceneLoader, m_selectedSaveProfile, token);
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
            using var writer = new BinaryWriter(new MemoryStream());
            SerializableObjects.RemoveAll(serializable =>
                serializable == null || serializable is Object unitySerializable && unitySerializable == null
            );

            AsyncSerializableObjects.RemoveAll(serializable =>
                serializable == null || serializable is Object unitySerializable && unitySerializable == null
            );

            var completedTasks = 0;
            int tasksCount = SerializableObjects.Count + AsyncSerializableObjects.Count;

            foreach (IRuntimeSerializable obj in SerializableObjects) {
                obj.Serialize(writer);
                ReportProgress(ref completedTasks, tasksCount, m_saveProgress);
            }

            foreach (IAsyncRuntimeSerializable obj in AsyncSerializableObjects) {
                await obj.Serialize(writer);
                ReportProgress(ref completedTasks, tasksCount, m_saveProgress);
            }

            await writer.WriteDataToFileAsync(m_dataPath, token);
        }


        private static async UniTask SaveProfileData (SaveProfile profile, CancellationToken token) {
            using var writer = new BinaryWriter(new MemoryStream());
            await profile.SerializeScope(writer);

            string dataPath = Path.Combine(profile.DataPath, $"{profile.Name}.profiledata");
            await writer.WriteDataToFileAsync(dataPath, token);
        }


        private static async UniTask SaveSceneData (
            SceneLoader sceneLoader, SaveProfile context, CancellationToken token
        ) {
            using var writer = new BinaryWriter(new MemoryStream());
            await sceneLoader.SerializeScope(writer);

            string dataPath = Path.Combine(context.DataPath, $"{sceneLoader.sceneName}.scenedata");
            await writer.WriteDataToFileAsync(dataPath, token);
        }


        private static async UniTask<HandlingResult> LoadGlobalData (CancellationToken token) {
            if (!File.Exists(m_dataPath))
                return HandlingResult.FileNotExists;

            try {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield(token);

                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(m_dataPath, token);

                var completedTasks = 0;
                int tasksCount = SerializableObjects.Count + AsyncSerializableObjects.Count;

                foreach (IRuntimeSerializable serializable in SerializableObjects) {
                    serializable.Deserialize(reader);
                    ReportProgress(ref completedTasks, tasksCount, m_loadProgress);
                }

                foreach (IAsyncRuntimeSerializable serializable in AsyncSerializableObjects) {
                    await serializable.Deserialize(reader);
                    ReportProgress(ref completedTasks, tasksCount, m_loadProgress);
                }

                Logger.Log("All registered objects was loaded");
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Loading operation was canceled");
                return HandlingResult.Canceled;
            }
        }


        private static void ReportProgress (ref int completedTasks, int tasksCount, IProgress<float> progress) {
            progress?.Report((float)++completedTasks / tasksCount);
        }

    }

}