using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal;
using SaveSystem.Internal.Templates;
using SaveSystem.Security;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Logger = SaveSystem.Internal.Logger;
using Object = UnityEngine.Object;

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

        internal const string SaveSystemFolder = "save_system_internal";
        private const string ProfileMetadataExtension = ".profilemetadata";

        private static string m_profilesFolderPath;

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

    #if ENABLE_LEGACY_INPUT_MANAGER
        private static KeyCode m_quickSaveKey;
    #endif

    #if ENABLE_INPUT_SYSTEM
        private static InputAction m_quickSaveAction;
    #endif

        private static readonly SynchronizationPoint SynchronizationPoint = new();

        private static bool m_autoSaveEnabled;
        private static float m_autoSaveLastTime;
        private static bool m_savedBeforeExit;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize () {
            m_handler = new SaveDataHandler {
                SerializationScope = m_globalScope = new SerializationScope {
                    Name = "Global scope"
                }
            };
            m_selectedSaveProfile = new DefaultSaveProfile();

            SetPlayerLoop();
            SetSettings(ResourcesManager.LoadSettings<SaveSystemSettings>());
            SetInternalSavedPaths();
            SetOnExitPlayModeCallback();

            m_exitCancellation = new CancellationTokenSource();
            Logger.Log(nameof(SaveSystemCore), "Initialized");
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
            DataPath = Storage.PrepareBeforeUsing(settings.dataPath, false);
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

            SelectedSaveProfile.Encrypt = settings.encryption;
            if (SelectedSaveProfile.Cryptographer == null)
                SelectedSaveProfile.Cryptographer = Cryptographer;
            else
                SelectedSaveProfile.Cryptographer.SetSettings(settings.encryptionSettings);
        }


        private static void SetAuthSettings (SaveSystemSettings settings) {
            AuthenticationSettings authSettings = settings.authenticationSettings;

            if (authSettings == null) {
                if (settings.authentication)
                    Logger.LogError(nameof(SaveSystemCore), "Authentication enabled but settings not set");
                return;
            }

            Authentication = settings.authentication;
            AuthManager = new AuthenticationManager(authSettings.globalAuthHashKey, authSettings.hashAlgorithm);

            SelectedSaveProfile.Authenticate = settings.authentication;
            SelectedSaveProfile.AuthManager = new AuthenticationManager(
                authSettings.profileAuthHashKey, authSettings.hashAlgorithm
            );
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


        private static async UniTask<HandlingResult> CommonSavingTask (SaveType saveType, CancellationToken token) {
            OnSaveStart?.Invoke(saveType);
            HandlingResult result = await SaveObjects(token);
            OnSaveEnd?.Invoke(saveType);

            LogResult(saveType, result);
            return result;
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
                await SaveGlobalData(DataPath, token);
                await m_selectedSaveProfile.SaveProfileData(m_selectedSaveProfile.DataPath, token);

                SceneSerializationContext[] contexts =
                    Object.FindObjectsByType<SceneSerializationContext>(FindObjectsSortMode.None);

                foreach (SceneSerializationContext sceneLoader in contexts)
                    await sceneLoader.SaveSceneData(sceneLoader.DataPath, token);

                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Saving operation canceled");
                return HandlingResult.Canceled;
            }
        }


        private static async UniTask ExecuteOnSceneLoadSaving (CancellationToken token) {
            await SynchronizationPoint.ExecuteTask(async () => await CommonSavingTask(SaveType.OnSceneLoad, token));
        }


        private static async UniTask<HandlingResult> SaveGlobalData (
            Func<UniTask<HandlingResult>> saving, CancellationToken token
        ) {
            return await CancelableOperationsHandler.Execute(
                saving, nameof(SaveSystemCore), "Global data saving canceled", token: token
            );
        }


        private static async UniTask<HandlingResult> LoadGlobalData (
            Func<UniTask<HandlingResult>> loading, CancellationToken token
        ) {
            return await CancelableOperationsHandler.Execute(
                loading, nameof(SaveSystemCore), "Global data loading canceled", token: token
            );
        }

    }

}