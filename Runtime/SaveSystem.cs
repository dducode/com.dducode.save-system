#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Templates;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Logger = SaveSystemPackage.Internal.Logger;
using MemoryStream = System.IO.MemoryStream;

// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    /// <summary>
    /// The Core of the Save System. It accepts <see cref="DynamicObjectGroup{TDynamic}">object group</see>
    /// and starts saving in three main modes - autosave, quick-save and save at checkpoint.
    /// Also it starts the saving when the player exit the game
    /// </summary>
    public static partial class SaveSystem {

        private const string AllProfilesFile = "all-profiles.data";
        private const string AllScreenshotsFile = "all-screenshots.data";

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
        private static SaveEvents m_enabledSaveEvents;
        private static float m_savePeriod;
        private static float m_autoSaveTime;
        private static string m_dataPath;

        // Checkpoints settings
        private static string m_playerTag;

        /// It will be canceled before exit game
        private static CancellationTokenSource m_exitCancellation;

        private static readonly SynchronizationPoint SynchronizationPoint = new();

        private static string m_profilesFolder;
        private static string m_scenesFolder;

    #if ENABLE_BOTH_SYSTEMS
        private static UsedInputSystem m_usedInputSystem;
    #endif

        private static bool m_periodicSaveEnabled;
        private static float m_periodicSaveLastTime;

        private static bool m_autoSaveEnabled;
        private static float m_autoSaveLastTime;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoInit () {
            SaveSystemSettings settings = ResourcesManager.LoadSettings();

            if (settings != null && settings.automaticInitialize) {
                settings.Dispose();
                Initialize();
            }
        }


        private static void SetPlayerLoop () {
            PlayerLoopSystem modifiedLoop = PlayerLoop.GetCurrentPlayerLoop();
            var saveSystemLoop = new PlayerLoopSystem {
                type = typeof(SaveSystem),
                updateDelegate = UpdateSystem
            };

            if (PlayerLoopManager.TryInsertSubSystem(ref modifiedLoop, saveSystemLoop, typeof(PreLateUpdate)))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError(nameof(SaveSystem), $"Failed insert system: {saveSystemLoop}");
        }


        private static void SetSettings (SaveSystemSettings settings) {
            if (settings == null) {
                settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
                Debug.LogWarning(Logger.FormattedMessage(
                    nameof(SaveSystem), Messages.SettingsNotFound
                ));
            }

            EnabledSaveEvents = settings.enabledSaveEvents;
            Logger.EnabledLogs = settings.enabledLogs;
            SavePeriod = settings.savePeriod;
            AutoSaveTime = settings.autoSaveTime;
            PlayerTag = settings.playerTag;

            SetupUserInputs(settings);

            Game.DataPath = settings.dataPath;
            Game.Encrypt = settings.encryption;
            Game.Authenticate = settings.authentication;

            settings.Dispose();
        }


        private static void SetupUserInputs (SaveSystemSettings settings) {
        #if ENABLE_BOTH_SYSTEMS
            m_usedInputSystem = settings.usedInputSystem;

            switch (m_usedInputSystem) {
                case UsedInputSystem.LegacyInputManager:
                    QuickSaveKey = settings.quickSaveKey;
                    ScreenCaptureKey = settings.screenCaptureKey;
                    break;
                case UsedInputSystem.InputSystem:
                    if (settings.inputActionAsset == null)
                        throw new ArgumentNullException(nameof(settings.inputActionAsset));
                    if (string.IsNullOrEmpty(settings.quickSaveId))
                        throw new ArgumentNullException(nameof(settings.quickSaveId));
                    if (string.IsNullOrEmpty(settings.screenCaptureId))
                        throw new ArgumentNullException(nameof(settings.screenCaptureId));
                    QuickSaveAction = settings.inputActionAsset.FindAction(settings.quickSaveId);
                    ScreenCaptureAction = settings.inputActionAsset.FindAction(settings.screenCaptureId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        #else
        #if ENABLE_LEGACY_INPUT_MANAGER
            QuickSaveKey = settings.quickSaveKey;
            ScreenCaptureKey = settings.screenCaptureKey;
        #endif

        #if ENABLE_INPUT_SYSTEM
            if (settings.inputActionAsset == null)
                throw new ArgumentNullException(nameof(settings.inputActionAsset));
            if (string.IsNullOrEmpty(settings.quickSaveId))
                throw new ArgumentNullException(nameof(settings.quickSaveId));
            if (string.IsNullOrEmpty(settings.screenCaptureId))
                throw new ArgumentNullException(nameof(settings.screenCaptureId));
            QuickSaveAction = settings.inputActionAsset.FindAction(settings.quickSaveId);
            ScreenCaptureAction = settings.inputActionAsset.FindAction(settings.screenCaptureId);
        #endif
        #endif
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
            string path = Path.Combine(InternalFolder, $"{newName}.profilemetadata");
            if (!string.IsNullOrEmpty(oldName))
                File.Move(Path.Combine(InternalFolder, $"{oldName}.profilemetadata"), path);

            using var writer = new SaveWriter(File.Open(path, FileMode.Open));
            writer.Write(newName);
            writer.Write(profile.Encrypt);
            writer.Write(profile.Authenticate);
        }


        private static void UpdateSystem () {
            if (m_exitCancellation.IsCancellationRequested)
                return;

            /*
             * Call saving request only in the one state machine
             * This is necessary to prevent sharing of the same file
             */
            SynchronizationPoint.ExecuteScheduledTask(m_exitCancellation.Token);

            UpdateUserInputs();

            if (m_periodicSaveEnabled)
                PeriodicSave();

            if (m_autoSaveEnabled)
                AutoSave();
        }


        private static void UpdateUserInputs () {
        #if ENABLE_BOTH_SYSTEMS
            switch (m_usedInputSystem) {
                case UsedInputSystem.LegacyInputManager:
                    CheckPressedKeys();
                    break;
                case UsedInputSystem.InputSystem:
                    CheckPerformedActions();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        #else
        #if ENABLE_LEGACY_INPUT_MANAGER
            CheckPressedKeys();
        #endif

        #if ENABLE_INPUT_SYSTEM
            CheckPerformedActions();
        #endif
        #endif
        }


    #if ENABLE_LEGACY_INPUT_MANAGER
        private static void CheckPressedKeys () {
            if (Input.GetKeyDown(QuickSaveKey))
                QuickSave();
            if (Input.GetKeyDown(ScreenCaptureKey))
                CaptureScreenshot();
        }
    #endif


    #if ENABLE_INPUT_SYSTEM
        private static void CheckPerformedActions () {
            if (QuickSaveAction != null && QuickSaveAction.WasPerformedThisFrame())
                QuickSave();
            if (ScreenCaptureAction != null && ScreenCaptureAction.WasPerformedThisFrame())
                CaptureScreenshot();
        }
    #endif


        private static void QuickSave () {
            ScheduleSave(SaveType.QuickSave);
        }


        private static void PeriodicSave () {
            if (m_periodicSaveLastTime + SavePeriod < Time.time) {
                ScheduleSave(SaveType.PeriodicSave);
                m_periodicSaveLastTime = Time.time;
            }
        }


        private static void AutoSave () {
            if (m_autoSaveLastTime + AutoSaveTime < Time.time) {
                if (Game.HasChanges) {
                    ScheduleSave(SaveType.AutoSave);
                    return;
                }

                if (Game.SaveProfile != null) {
                    if (Game.SaveProfile.HasChanges)
                        ScheduleSave(SaveType.AutoSave);
                    else if (Game.SaveProfile.SceneContext != null && Game.SaveProfile.SceneContext.HasChanges)
                        ScheduleSave(SaveType.AutoSave);
                }
                else if (Game.SceneContext != null && Game.SceneContext.HasChanges) {
                    ScheduleSave(SaveType.AutoSave);
                }

                m_autoSaveLastTime = Time.time;
            }
        }


        private static void SetupEvents (SaveEvents enabledSaveEvents) {
            m_periodicSaveEnabled = false;
            m_autoSaveEnabled = false;
            Application.focusChanged -= OnFocusLost;
            Application.lowMemory -= OnLowMemory;

            switch (enabledSaveEvents) {
                case SaveEvents.None:
                    break;
                case not SaveEvents.All:
                    m_periodicSaveEnabled = enabledSaveEvents.HasFlag(SaveEvents.PeriodicSave);
                    m_autoSaveEnabled = enabledSaveEvents.HasFlag(SaveEvents.AutoSave);
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnFocusLost))
                        Application.focusChanged += OnFocusLost;
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnLowMemory))
                        Application.lowMemory += OnLowMemory;
                    break;
                case SaveEvents.All:
                    m_periodicSaveEnabled = true;
                    m_autoSaveEnabled = true;
                    Application.focusChanged += OnFocusLost;
                    Application.lowMemory += OnLowMemory;
                    break;
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
            HandlingResult result;

            try {
                token.ThrowIfCancellationRequested();
                await Game.Save(token);
                result = HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                result = HandlingResult.Canceled;
            }
            catch {
                result = HandlingResult.Error;
            }

            OnSaveEnd?.Invoke(saveType);
            LogResult(saveType, result);
        }


        private static void LogResult (SaveType saveType, HandlingResult result) {
            if (result is HandlingResult.Success)
                Logger.Log(nameof(SaveSystem), $"{saveType}: success");
            else if (result is HandlingResult.Canceled)
                Logger.LogWarning(nameof(SaveSystem), $"{saveType}: canceled");
            else if (result is HandlingResult.Error)
                Logger.LogError(nameof(SaveSystem), $"{saveType}: error");
        }


        private static async UniTask UploadToCloudStorage (
            ICloudStorage cloudStorage, CancellationToken token = default
        ) {
            StorageData gameData = await Game.ExportGameData(token);
            if (gameData != null)
                await cloudStorage.Push(gameData);

            string[] profiles = Directory.GetFileSystemEntries(InternalFolder, "*.profilemetadata");
            if (profiles.Length > 0)
                await UploadProfiles(cloudStorage, profiles, token);

            StorageData dataTable = await DataTable.Export(token);
            if (dataTable != null)
                await cloudStorage.Push(dataTable);

            string[] screenshots = Directory.GetFileSystemEntries(ScreenshotsFolder, "*.png");
            if (screenshots.Length > 0)
                await UploadScreenshots(cloudStorage, screenshots, token);
        }


        private static async UniTask UploadProfiles (
            ICloudStorage cloudStorage, string[] paths, CancellationToken token
        ) {
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);
            writer.Write(paths.Length);

            foreach (string path in paths) {
                writer.Write(Path.GetFileName(path));
                await using var reader = new SaveReader(File.Open(path, FileMode.Open));
                var profile = new SaveProfile(
                    reader.ReadString(), reader.Read<bool>(), reader.Read<bool>()
                );

                writer.Write(profile.Name);
                writer.Write(profile.Encrypt);
                writer.Write(profile.Authenticate);
                writer.Write(await profile.ExportProfileData(token));
            }

            await cloudStorage.Push(new StorageData(memoryStream.ToArray(), AllProfilesFile));
        }


        private static async UniTask UploadScreenshots (
            ICloudStorage cloudStorage, string[] paths, CancellationToken token
        ) {
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);
            writer.Write(paths.Length);

            foreach (string path in paths) {
                writer.Write(Path.GetFileName(path));
                writer.Write(await File.ReadAllBytesAsync(path, token));
            }

            await cloudStorage.Push(new StorageData(memoryStream.ToArray(), AllScreenshotsFile));
        }


        private static async UniTask DownloadFromCloudStorage (
            ICloudStorage cloudStorage, CancellationToken token = default
        ) {
            StorageData gameData = await cloudStorage.Pull(Path.GetFileName(Game.DataPath));
            if (gameData != null)
                await Game.ImportGameData(gameData.rawData, token);

            StorageData profiles = await cloudStorage.Pull(AllProfilesFile);
            if (profiles != null)
                await DownloadProfiles(profiles);

            StorageData dataTable = await cloudStorage.Pull(Path.GetFileName(DataTable.Path));
            if (dataTable != null)
                await DataTable.Import(dataTable.rawData, token);

            StorageData screenshots = await cloudStorage.Pull(AllScreenshotsFile);
            if (screenshots != null)
                await DownloadScreenshots(screenshots);
        }


        private static async UniTask DownloadProfiles (StorageData profiles) {
            await using var reader = new SaveReader(new MemoryStream(profiles.rawData));
            var count = reader.Read<int>();

            for (var i = 0; i < count; i++) {
                string path = Path.Combine(InternalFolder, reader.ReadString());
                await using var writer = new SaveWriter(File.Open(path, FileMode.OpenOrCreate));
                var profile = new SaveProfile(
                    reader.ReadString(), reader.Read<bool>(), reader.Read<bool>()
                );
                writer.Write(profile.Name);
                writer.Write(profile.Encrypt);
                writer.Write(profile.Authenticate);
                await profile.ImportProfileData(reader.ReadArray<byte>());
            }
        }


        private static async UniTask DownloadScreenshots (StorageData screenshots) {
            await using var reader = new SaveReader(new MemoryStream(screenshots.rawData));
            var count = reader.Read<int>();

            for (var i = 0; i < count; i++) {
                await File.WriteAllBytesAsync(
                    Path.Combine(ScreenshotsFolder, reader.ReadString()), reader.ReadArray<byte>()
                );
            }
        }

    }

}