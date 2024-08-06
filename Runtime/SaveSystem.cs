#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Templates;
using SaveSystemPackage.Serialization;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using File = SaveSystemPackage.Internal.File;
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

        internal static event Action OnUpdateSystem;

        /// It will be canceled before exit game
        private static CancellationTokenSource s_exitCancellation;

        private static readonly SynchronizationPoint s_synchronizationPoint = new();

        private static bool s_periodicSaveEnabled;
        private static float s_periodicSaveLastTime;

        private static bool s_autoSaveEnabled;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoInit () {
            SaveSystemSettings settings = SaveSystemSettings.Load();

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

            Settings = settings;
        }


        static partial void SetOnExitPlayModeCallback ();


        internal static void SaveAtCheckpoint (Component other) {
            if (!other.CompareTag(Settings.PlayerTag))
                return;

            ScheduleSave(SaveType.SaveAtCheckpoint);
        }


        private static void UpdateSystem () {
            if (s_exitCancellation.IsCancellationRequested)
                return;

            /*
             * Call saving request only in the one state machine
             * This is necessary to prevent sharing of the same file
             */
            s_synchronizationPoint.ExecuteScheduledTask(s_exitCancellation.Token);

            UpdateUserInputs();

            if (s_periodicSaveEnabled)
                PeriodicSave();

            if (s_autoSaveEnabled)
                AutoSave();

            OnUpdateSystem?.Invoke();
        }


        private static void UpdateUserInputs () {
        #if ENABLE_BOTH_SYSTEMS
            switch (Settings.UsedInputSystem) {
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
            if (Input.GetKeyDown(Settings.QuickSaveKey))
                QuickSave();
            if (Input.GetKeyDown(Settings.ScreenCaptureKey))
                CaptureScreenshot();
        }
    #endif


    #if ENABLE_INPUT_SYSTEM
        private static void CheckPerformedActions () {
            if (Settings.QuickSaveAction != null &&
                Settings.QuickSaveAction.WasPerformedThisFrame())
                QuickSave();
            if (Settings.ScreenCaptureAction != null &&
                Settings.ScreenCaptureAction.WasPerformedThisFrame())
                CaptureScreenshot();
        }
    #endif


        private static void QuickSave () {
            ScheduleSave(SaveType.QuickSave);
        }


        private static void PeriodicSave () {
            if (s_periodicSaveLastTime + Settings.SavePeriod < Time.time) {
                ScheduleSave(SaveType.PeriodicSave);
                s_periodicSaveLastTime = Time.time;
            }
        }


        private static void AutoSave () {
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
        }


        private static void SetupEvents (SaveEvents enabledSaveEvents) {
            s_periodicSaveEnabled = false;
            s_autoSaveEnabled = false;
            Application.focusChanged -= OnFocusLost;
            Application.lowMemory -= OnLowMemory;

            switch (enabledSaveEvents) {
                case SaveEvents.None:
                    break;
                case not SaveEvents.All:
                    s_periodicSaveEnabled = enabledSaveEvents.HasFlag(SaveEvents.PeriodicSave);
                    s_autoSaveEnabled = enabledSaveEvents.HasFlag(SaveEvents.AutoSave);
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnFocusLost))
                        Application.focusChanged += OnFocusLost;
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnLowMemory))
                        Application.lowMemory += OnLowMemory;
                    break;
                case SaveEvents.All:
                    s_periodicSaveEnabled = true;
                    s_autoSaveEnabled = true;
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
            s_synchronizationPoint.ScheduleTask(async token => await CommonSavingTask(saveType, token));
        }


        private static async Task CommonSavingTask (SaveType saveType, CancellationToken token) {
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


        private static async Task UploadToCloudStorage (CancellationToken token = default) {
            StorageData gameData = await Game.ExportGameData(token);
            if (gameData != null)
                await CloudStorage.Push(gameData);

            await UploadProfiles(CloudStorage, token);

            if (Storage.ScreenshotsDirectoryExists())
                await UploadScreenshots(CloudStorage, token);
        }


        private static async Task UploadProfiles (ICloudStorage cloudStorage, CancellationToken token) {
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);
            File[] profiles = Storage.InternalDirectory.EnumerateFiles("profile").ToArray();
            if (profiles.Length == 0)
                return;

            writer.Write(profiles.Length);

            foreach (File file in profiles) {
                writer.Write(file.Name);
                await using var reader = new SaveReader(file.Open());
                string typeName = reader.ReadString();
                var type = Type.GetType(typeName);

                if (type == null) {
                    Logger.LogWarning(nameof(SaveSystem), $"Type {typeName} is not found");
                    continue;
                }

                var profile = (SaveProfile)Activator.CreateInstance(type);
                InitializeProfile(profile, reader.ReadString(), reader.Read<bool>(), reader.Read<bool>());
                SerializationManager.DeserializeGraph(reader, profile);
                SerializeProfile(writer, profile);
                writer.Write(await profile.ExportProfileData(token));
            }

            await cloudStorage.Push(new StorageData(memoryStream.ToArray(), SaveSystemConstants.AllProfilesFile));
        }


        private static async Task UploadScreenshots (ICloudStorage cloudStorage, CancellationToken token) {
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);
            File[] screenshots = Storage.ScreenshotsDirectory.EnumerateFiles("png").ToArray();
            if (screenshots.Length == 0)
                return;

            writer.Write(screenshots.Length);

            foreach (File screenshot in screenshots) {
                writer.Write(screenshot.Name);
                writer.Write(await screenshot.ReadAllBytesAsync(token));
            }

            await cloudStorage.Push(new StorageData(memoryStream.ToArray(), SaveSystemConstants.AllScreenshotsFile));
        }


        private static async Task DownloadFromCloudStorage (CancellationToken token = default) {
            StorageData gameData = await CloudStorage.Pull(Game.DataFile.Name);
            if (gameData != null)
                await Game.ImportGameData(gameData.rawData, token);

            StorageData profiles = await CloudStorage.Pull(SaveSystemConstants.AllProfilesFile);
            if (profiles != null)
                await DownloadProfiles(profiles);

            StorageData screenshots = await CloudStorage.Pull(SaveSystemConstants.AllScreenshotsFile);
            if (screenshots != null)
                await DownloadScreenshots(screenshots);
        }


        private static async Task DownloadProfiles (StorageData profiles) {
            await using var reader = new SaveReader(new MemoryStream(profiles.rawData));
            var count = reader.Read<int>();

            for (var i = 0; i < count; i++) {
                File file = Storage.InternalDirectory.GetOrCreateFile(reader.ReadString(), "profile");
                await using var writer = new SaveWriter(file.Open());
                string typeName = reader.ReadString();
                var type = Type.GetType(typeName);

                if (type == null) {
                    Logger.LogWarning(nameof(SaveSystem), $"Type {typeName} is not found");
                    continue;
                }

                var profile = (SaveProfile)Activator.CreateInstance(type);
                InitializeProfile(profile, reader.ReadString(), reader.Read<bool>(), reader.Read<bool>());
                SerializationManager.DeserializeGraph(reader, profile);
                SerializeProfile(writer, profile);
                await profile.ImportProfileData(reader.ReadArray<byte>());
            }
        }


        private static async Task DownloadScreenshots (StorageData screenshots) {
            await using var reader = new SaveReader(new MemoryStream(screenshots.rawData));
            var count = reader.Read<int>();

            for (var i = 0; i < count; i++) {
                await Storage.ScreenshotsDirectory
                   .GetOrCreateFile(reader.ReadString(), "png")
                   .WriteAllBytesAsync(reader.ReadArray<byte>());
            }
        }

    }

}