using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.Internal;
using UnityEngine;
using Logger = SaveSystemPackage.Internal.Logger;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class SaveSystem {

        public static Game Game { get; private set; }

        /// <summary>
        /// It's used to manage autosave loop, save on focus changed, on low memory and on quitting the game
        /// </summary>
        /// <example> EnabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnFocusChanged </example>
        /// <seealso cref="SaveEvents"/>
        public static SaveEvents EnabledSaveEvents {
            get => m_enabledSaveEvents;
            set => SetupEvents(m_enabledSaveEvents = value);
        }

        /// <summary>
        /// It's used to enable logs
        /// </summary>
        /// <example> EnabledLogs = LogLevel.Warning | LogLevel.Error </example>
        /// <seealso cref="LogLevel"/>
        public static LogLevel EnabledLogs {
            get => Logger.EnabledLogs;
            set => Logger.EnabledLogs = value;
        }

        /// <summary>
        /// It's used to determine periodic saving frequency
        /// </summary>
        /// <value> Saving period in seconds </value>
        /// <remarks> If it equals 0, saving will be executed at every frame </remarks>
        public static float SavePeriod {
            get => m_savePeriod;
            set {
                if (value < 0) {
                    throw new ArgumentException(
                        "Save period cannot be less than 0.", nameof(SavePeriod)
                    );
                }

                m_savePeriod = value;
            }
        }

        /// <summary>
        /// It's used to determine auto save frequency
        /// </summary>
        /// <value> Saving period in seconds </value>
        /// <remarks> If it equals 0, saving will be executed at every frame </remarks>
        public static float AutoSaveTime {
            get => m_autoSaveTime;
            set {
                if (value < 0) {
                    throw new ArgumentException(
                        "Auto save time cannot be less than 0.", nameof(AutoSaveTime)
                    );
                }

                m_autoSaveTime = value;
            }
        }

        /// <summary>
        /// Player tag is used to filtering messages from triggered checkpoints
        /// </summary>
        /// <value> Tag of the player object </value>
        [NotNull]
        public static string PlayerTag {
            get => m_playerTag;
            set {
                if (string.IsNullOrEmpty(value)) {
                    throw new ArgumentNullException(
                        nameof(PlayerTag), "Player tag cannot be null or empty"
                    );
                }

                m_playerTag = value;
            }
        }

    #if ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Binds any key to quick save
        /// </summary>
        public static KeyCode QuickSaveKey { get; set; }
    #endif

    #if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Binds any input action to quick save
        /// </summary>
        public static InputAction QuickSaveAction { get; set; }
    #endif

        /// <summary>
        /// The event is called before saving. It can be useful when you use async saving
        /// </summary>
        /// <value>
        /// Listeners will be called when the system starts saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveStart;

        /// <summary>
        /// The event is called after saving
        /// </summary>
        /// <value>
        /// Listeners will be called when the system completes saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveEnd;


        public static void Initialize () {
            Game = new Game();

            SetPlayerLoop();
            SetSettings(ResourcesManager.LoadSettings());
            SetInternalFolder();
            SetOnExitPlayModeCallback();

            m_exitCancellation = new CancellationTokenSource();
            Logger.Log(nameof(SaveSystem), "Initialized");
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureSettings (SaveSystemSettings settings) {
            SetSettings(settings);
            Logger.Log(nameof(SaveSystem), $"Parameters was configured: {settings}");
        }


        /// <summary>
        /// Save the game and load a scene
        /// </summary>
        public static async UniTask LoadSceneAsync (
            Func<UniTask> sceneLoading, CancellationToken token = default
        ) {
            await SynchronizationPoint.ExecuteTask(async () => await Game.Save(token));
            await SceneLoader.LoadSceneAsync(sceneLoading);
        }


        /// <summary>
        /// Save the game and load a scene
        /// </summary>
        public static async UniTask LoadSceneAsync<TData> (
            Func<UniTask> sceneLoading, TData passedData, CancellationToken token = default
        ) {
            await SynchronizationPoint.ExecuteTask(async () => await Game.Save(token));
            await SceneLoader.LoadSceneAsync(sceneLoading, passedData);
        }


        /// <summary>
        /// Save the game and exit
        /// </summary>
        /// <param name="exitCode"> if the exit code is zero, the game will be saved </param>
        public static async UniTask ExitGame (int exitCode = 0) {
            SynchronizationPoint.Clear();
            m_exitCancellation.Cancel();
            if (exitCode == 0)
                await SynchronizationPoint.ExecuteTask(async () => await Game.Save());

        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit(exitCode);
        #endif
        }


        public static async UniTask UploadToCloud (
            [NotNull] ICloudStorage cloudStorage, CancellationToken token = default
        ) {
            if (cloudStorage == null)
                throw new ArgumentNullException(nameof(cloudStorage));

            try {
                token.ThrowIfCancellationRequested();
                await SynchronizationPoint.ExecuteTask(async () => await UploadToCloudStorage(cloudStorage, token));
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystem), "Push to cloud canceled");
            }
        }


        public static async UniTask DownloadFromCloud (
            [NotNull] ICloudStorage cloudStorage, CancellationToken token = default
        ) {
            if (cloudStorage == null)
                throw new ArgumentNullException(nameof(cloudStorage));

            try {
                token.ThrowIfCancellationRequested();
                await SynchronizationPoint.ExecuteTask(async () => await DownloadFromCloudStorage(cloudStorage, token));
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystem), "Pull from cloud canceled");
            }
        }

    }

}