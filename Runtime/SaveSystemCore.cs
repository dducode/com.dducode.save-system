using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.CloudSave;
using SaveSystem.Internal;
using SaveSystem.Internal.Templates;
using SaveSystem.Security;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Logger = SaveSystem.Internal.Logger;
using MemoryStream = System.IO.MemoryStream;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystem {

    /// <summary>
    /// The Core of the Save System. It accepts <see cref="DynamicObjectGroup{TDynamic}">object group</see>
    /// and starts saving in three main modes - autosave, quick-save and save at checkpoint.
    /// Also it starts the saving when the player exit the game
    /// </summary>
    public static partial class SaveSystemCore {

        private const string AllProfilesFile = "all-profiles.data";

        internal static readonly string InternalFolder = SetInternalFolder();

        internal static string ProfilesFolder {
            get {
                if (string.IsNullOrEmpty(m_profilesFolder)) {
                    m_profilesFolder = Storage.PrepareBeforeUsing("profiles");
                    Directory.CreateDirectory(m_profilesFolder);
                }

                return m_profilesFolder;
            }
        }

        internal static string ScenesFolder {
            get {
                if (string.IsNullOrEmpty(m_scenesFolder)) {
                    m_scenesFolder = Storage.PrepareBeforeUsing("scenes");
                    Directory.CreateDirectory(m_scenesFolder);
                }

                return m_scenesFolder;
            }
        }

        // Common settings
        private static SaveProfile m_selectedSaveProfile;
        private static SaveEvents m_enabledSaveEvents;
        private static float m_savePeriod;
        private static string m_dataPath;

        // Checkpoints settings
        private static string m_playerTag;

        /// It will be canceled before exit game
        private static CancellationTokenSource m_exitCancellation;

        private static SerializationScope m_globalScope;
        private static SaveDataHandler m_handler;
        private static ICloudStorage m_cloudStorage;

    #if ENABLE_LEGACY_INPUT_MANAGER
        private static KeyCode m_quickSaveKey;
    #endif

    #if ENABLE_INPUT_SYSTEM
        private static InputAction m_quickSaveAction;
    #endif

        private static readonly SynchronizationPoint SynchronizationPoint = new();

        private static string m_profilesFolder;
        private static string m_scenesFolder;

        private static bool m_autoSaveEnabled;
        private static float m_autoSaveLastTime;
        private static bool m_savedBeforeExit;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoInit () {
            SaveSystemSettings settings = ResourcesManager.LoadSettings();
            if (settings != null && settings.automaticInitialize)
                Initialize();
        }


        private static void SetPlayerLoop () {
            PlayerLoopSystem modifiedLoop = PlayerLoop.GetCurrentPlayerLoop();
            var saveSystemLoop = new PlayerLoopSystem {
                type = typeof(SaveSystemCore),
                updateDelegate = UpdateSystem
            };

            if (PlayerLoopManager.TryInsertSubSystem(ref modifiedLoop, saveSystemLoop, typeof(PreLateUpdate)))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError(nameof(SaveSystemCore), $"Failed insert system: {saveSystemLoop}");
        }


        private static void SetSettings (SaveSystemSettings settings) {
            if (settings == null) {
                settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
                Debug.LogWarning(Logger.FormattedMessage(
                    nameof(SaveSystemCore), Messages.SettingsNotFound
                ));
            }

            SetCommonSettings(settings);
            SetCheckpointsSettings(settings);
            SetEncryptionSettings(settings);
            SetAuthSettings(settings);
        }


        private static void SetCommonSettings (SaveSystemSettings settings) {
            EnabledSaveEvents = settings.enabledSaveEvents;
            Logger.EnabledLogs = settings.enabledLogs;
            SavePeriod = settings.savePeriod;
            DataPath = Storage.PrepareBeforeUsing(settings.dataPath);
        }


        private static void SetCheckpointsSettings (SaveSystemSettings settings) {
            PlayerTag = settings.playerTag;
        }


        private static void SetEncryptionSettings (SaveSystemSettings settings) {
            if (settings.encryptionSettings == null) {
                if (settings.encryption)
                    Logger.LogError(nameof(SaveSystemCore), "Encryption enabled but settings not set");
                return;
            }

            Encrypt = settings.encryption;
            if (Cryptographer == null)
                Cryptographer = new Cryptographer(settings.encryptionSettings);
            else
                Cryptographer.SetSettings(settings.encryptionSettings);
        }


        private static void SetAuthSettings (SaveSystemSettings settings) {
            if (settings.authenticationSettings == null) {
                if (settings.authentication)
                    Logger.LogError(nameof(SaveSystemCore), "Authentication enabled but settings not set");
                return;
            }

            Authenticate = settings.authentication;
            if (AuthManager == null)
                AuthManager = new AuthenticationManager(settings.authenticationSettings);
            else
                AuthManager.SetSettings(settings.authenticationSettings);
        }


        private static string SetInternalFolder () {
            string folder = Storage.PrepareBeforeUsing(".internal");
            Directory.CreateDirectory(folder).Attributes |= FileAttributes.Hidden;
            return folder;
        }


        static partial void SetOnExitPlayModeCallback ();


        internal static void SaveAtCheckpoint (Component other) {
            if (!other.CompareTag(PlayerTag))
                return;

            ScheduleSave(SaveType.SaveAtCheckpoint);
        }


        internal static void UpdateProfile (SaveProfile profile, string oldName, string newName) {
            var memoryStream = new MemoryStream();
            using var writer = new SaveWriter(memoryStream);
            writer.Write(profile.GetType().Name);
            profile.Serialize(writer);
            byte[] data = memoryStream.ToArray();

            string path = Path.Combine(InternalFolder, $"{newName}.profile");
            if (!string.IsNullOrEmpty(oldName))
                File.Move(Path.Combine(InternalFolder, $"{oldName}.profile"), path);

            SynchronizationPoint.ScheduleTask(
                async token => await File.WriteAllBytesAsync(path, data, token).AsUniTask(),
                true
            );
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
                Logger.LogWarning(nameof(SaveSystemCore), "Saving before the quitting is not supported in the editor");
                return m_savedBeforeExit = true;
            }
            else {
                OnSaveStart?.Invoke(SaveType.OnExit);
                SaveObjects().ContinueWith(Continuation).Forget();
                return false;
            }

            void Continuation (HandlingResult result) {
                OnSaveEnd?.Invoke(SaveType.OnExit);
                m_savedBeforeExit = true;
                if (result is HandlingResult.Success)
                    Logger.Log(nameof(SaveSystemCore), "Successful saving before the quitting");
                else if (result is HandlingResult.Canceled)
                    Logger.LogWarning(nameof(SaveSystemCore), "The saving before the quitting was canceled");
                else if (result is HandlingResult.Error)
                    Logger.LogError(nameof(SaveSystemCore), "Some error was occured");
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
            SynchronizationPoint.ScheduleTask(async token => await CommonSavingTask(saveType, token));
        }


        private static async UniTask CommonSavingTask (SaveType saveType, CancellationToken token) {
            OnSaveStart?.Invoke(saveType);
            HandlingResult result = await SaveObjects(token);
            OnSaveEnd?.Invoke(saveType);

            LogResult(saveType, result);
        }


        private static void LogResult (SaveType saveType, HandlingResult result) {
            if (result is HandlingResult.Success)
                Logger.Log(nameof(SaveSystemCore), $"{saveType}: success");
            else if (result is HandlingResult.Canceled)
                Logger.LogWarning(nameof(SaveSystemCore), $"{saveType}: canceled");
            else if (result is HandlingResult.Error)
                Logger.LogError(nameof(SaveSystemCore), $"{saveType}: error");
        }


        private static async UniTask<HandlingResult> SaveObjects (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await SaveGlobalData(token);

                if (m_selectedSaveProfile != null)
                    await m_selectedSaveProfile.SaveProfileData(token);

                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Saving operation canceled");
                return HandlingResult.Canceled;
            }
        }


        private static async UniTask<HandlingResult> LoadObjects (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await LoadGlobalData(token);

                if (m_selectedSaveProfile != null)
                    await m_selectedSaveProfile.LoadProfileData(token);

                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Loading operation canceled");
                return HandlingResult.Canceled;
            }
        }


        /// <summary>
        /// Start saving of objects in the global scope and wait it
        /// </summary>
        private static async UniTask SaveGlobalData (CancellationToken token = default) {
            await CancelableOperationsHandler.Execute(
                async () => await m_handler.SaveData(DataPath, token),
                nameof(SaveSystemCore), "Global data saving canceled", token: token
            );
        }


        /// <summary>
        /// Start loading of objects in the global scope and wait it
        /// </summary>
        private static async UniTask LoadGlobalData (CancellationToken token = default) {
            await CancelableOperationsHandler.Execute(
                async () => await m_handler.LoadData(DataPath, token),
                nameof(SaveSystemCore), "Global data loading canceled", token: token
            );
        }


        private static async UniTask PushToCloudStorage (
            ICloudStorage cloudStorage, CancellationToken token = default
        ) {
            if (File.Exists(DataPath)) {
                cloudStorage.Push(new StorageData(
                    await File.ReadAllBytesAsync(DataPath, token), Path.GetFileName(DataPath))
                );
            }

            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profile");
            if (paths.Length > 0)
                await PushProfiles(cloudStorage, paths, token);

            if (File.Exists(DataTable.Path)) {
                cloudStorage.Push(new StorageData(
                    await File.ReadAllBytesAsync(DataTable.Path, token), Path.GetFileName(DataTable.Path))
                );
            }
        }


        private static async UniTask PushProfiles (
            ICloudStorage cloudStorage, string[] paths, CancellationToken token
        ) {
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);
            writer.Write(paths);

            foreach (string path in paths) {
                await using var reader = new SaveReader(File.Open(path, FileMode.Open));
                string type = reader.ReadString();
                var profile = (SaveProfile)Activator.CreateInstance(
                    Type.GetType(type) ?? throw new InvalidOperationException($"Unknown profile type: {type}")
                );
                profile.Deserialize(reader, reader.Read<int>());

                writer.Write(type);
                writer.Write(profile.Version);
                profile.Serialize(writer);
                writer.Write(await profile.ExportProfileData(token));
            }

            cloudStorage.Push(new StorageData(memoryStream.ToArray(), AllProfilesFile));
        }


        private static async UniTask PullFromCloudStorage (
            ICloudStorage cloudStorage, CancellationToken token = default
        ) {
            StorageData globalData = await cloudStorage.Pull(Path.GetFileName(DataPath));
            if (globalData != null)
                await File.WriteAllBytesAsync(DataPath, globalData.rawData, token);

            StorageData profiles = await cloudStorage.Pull(Path.GetFileName(AllProfilesFile));
            if (profiles != null)
                await PullProfiles(profiles);

            StorageData dataTable = await cloudStorage.Pull(Path.GetFileName(DataTable.Path));
            if (dataTable != null)
                await File.WriteAllBytesAsync(DataTable.Path, dataTable.rawData, token);
        }


        private static async UniTask PullProfiles (StorageData profiles) {
            await using var reader = new SaveReader(new MemoryStream(profiles.rawData));
            string[] paths = reader.ReadStringArray();

            foreach (string path in paths) {
                await using var writer = new SaveWriter(File.Open(path, FileMode.OpenOrCreate));
                string type = reader.ReadString();
                var profile = (SaveProfile)Activator.CreateInstance(
                    Type.GetType(type) ?? throw new InvalidOperationException($"Unknown profile type: {type}")
                );
                profile.Deserialize(reader, reader.Read<int>());

                writer.Write(type);
                writer.Write(profile.Version);
                profile.Serialize(writer);
                await profile.ImportProfileData(reader.ReadArray<byte>());
            }
        }


        private static async UniTask ExecuteOnSceneLoadSaving (CancellationToken token) {
            await SynchronizationPoint.ExecuteTask(async () => await CommonSavingTask(SaveType.OnSceneLoad, token));
        }

    }

}