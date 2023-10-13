using System;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = SaveSystem.Internal.Logger;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaveSystem.Core {

    public static partial class SaveSystemCore {

        /// <summary>
        /// It's used to manage autosave loop, save on focus changed, on low memory and on quitting the game
        /// </summary>
        /// <seealso cref="SaveEvents"/>
        public static SaveEvents EnabledSaveEvents {
            get => m_enabledSaveEvents;
            set {
                m_enabledSaveEvents = value;
                SetupEvents(m_enabledSaveEvents);
                if (DebugEnabled)
                    Logger.Log($"Set save events: {m_enabledSaveEvents}");
            }
        }

        /// <summary>
        /// It's used into autosave loop to determine saving frequency
        /// </summary>
        /// <value> Saving period in seconds </value>
        /// <remarks> If it equals 0, saving will be executed at every frame </remarks>
        public static float SavePeriod {
            get => m_savePeriod;
            set {
                if (value < 0) {
                    throw new ArgumentException(
                        Logger.FormattedMessage("Save period cannot be less than 0."), nameof(SavePeriod)
                    );
                }

                m_savePeriod = value;

                if (DebugEnabled)
                    Logger.Log($"Set save period: {m_savePeriod}");
            }
        }

        /// <summary>
        /// Configure it to set parallel saving handlers
        /// </summary>
        public static bool IsParallel {
            get => m_isParallel;
            set {
                m_isParallel = value;

                if (DebugEnabled)
                    Logger.Log(m_isParallel ? "Enable" : "Disable" + " parallel saving");
            }
        }

        /// <summary>
        /// Enables logs
        /// </summary>
        /// <remarks>
        /// It configures only simple logs, other logs (warnings and errors) will be written to console anyway.
        /// </remarks>
        public static bool DebugEnabled {
            get => m_debugEnabled;
            set {
                m_debugEnabled = value;

                if (DebugEnabled)
                    Logger.Log("Enable debug logs");
            }
        }

        /// <summary>
        /// Determines whether checkpoints will be destroyed after saving
        /// </summary>
        /// <value> If true, triggered checkpoint will be deleted from scene after saving </value>
        public static bool DestroyCheckPoints {
            get => m_destroyCheckPoints;
            set {
                m_destroyCheckPoints = value;

                if (DebugEnabled)
                    Logger.Log(m_destroyCheckPoints ? "Enable" : "Disable" + " destroying checkpoints");
            }
        }

        /// <summary>
        /// Player tag is used to filtering messages from triggered checkpoints
        /// </summary>
        /// <value> Tag of the player object </value>
        public static string PlayerTag {
            get => m_playerTag;
            set {
                if (string.IsNullOrEmpty(value)) {
                    throw new ArgumentNullException(
                        Logger.FormattedMessage("Player tag cannot be null or empty."), nameof(PlayerTag)
                    );
                }

                m_playerTag = value;

                if (DebugEnabled)
                    Logger.Log($"Set player tag: {m_playerTag}");
            }
        }

        /// <summary>
        /// Event that is called before saving. It can be useful when you use async saving
        /// </summary>
        /// <value>
        /// Listeners that will be called when core will start saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveStart {
            add {
                m_onSaveStart += value;

                if (DebugEnabled)
                    Logger.Log($"Listener {value.Method.Name} subscribe to {nameof(OnSaveStart)} event");
            }
            remove {
                m_onSaveStart -= value;

                if (DebugEnabled)
                    Logger.Log($"Listener {value.Method.Name} unsubscribe from {nameof(OnSaveStart)} event");
            }
        }

        /// <summary>
        /// Event that is called after saving
        /// </summary>
        /// <value>
        /// Listeners that will be called when core will finish saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveEnd {
            add {
                m_onSaveEnd += value;

                if (DebugEnabled)
                    Logger.Log($"Listener {value.Method.Name} subscribe to {nameof(OnSaveEnd)} event");
            }
            remove {
                m_onSaveEnd -= value;

                if (DebugEnabled)
                    Logger.Log($"Listener {value.Method.Name} unsubscribe from {nameof(OnSaveEnd)} event");
            }
        }


        /// <summary>
        /// Registers a handler to automatic save, quick-save, save at checkpoit and at save on exit
        /// </summary>
        public static void RegisterObjectHandler ([NotNull] IObjectHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Handlers.Add(handler);

            if (DebugEnabled)
                Logger.Log($"Register handler: {handler}");
        }


        /// <summary>
        /// Registers an async handler to automatic save, quick-save, save at checkpoit and at save on exit
        /// </summary>
        public static void RegisterAsyncObjectHandler ([NotNull] IAsyncObjectHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            AsyncHandlers.Add(handler);

            if (DebugEnabled)
                Logger.Log($"Register handler: {handler}");
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <param name="enabledSaveEvents"></param>
        /// <param name="isParallel"></param>
        /// <param name="debugEnabled"> <see cref="DebugEnabled"/> </param>
        /// <param name="destroyCheckPoints"> <see cref="DestroyCheckPoints"/> </param>
        /// <param name="playerTag"> <see cref="PlayerTag"/> </param>
        /// <param name="savePeriod"> <see cref="SavePeriod"/> </param>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureParameters (
            SaveEvents enabledSaveEvents,
            bool isParallel,
            bool debugEnabled,
            bool destroyCheckPoints,
            string playerTag,
            float savePeriod = 0
        ) {
            m_enabledSaveEvents = enabledSaveEvents;
            SetupEvents(m_enabledSaveEvents);
            m_isParallel = isParallel;
            m_debugEnabled = debugEnabled;
            m_destroyCheckPoints = destroyCheckPoints;
            m_playerTag = playerTag;
            m_savePeriod = savePeriod;

            if (DebugEnabled) {
                Logger.Log(
                    "Parameters was configured:" +
                    $"\nEnabled Save Events: {EnabledSaveEvents}" +
                    $"\nIs Parallel: {IsParallel}" +
                    $"\nDebug Enabled: {DebugEnabled}" +
                    $"\nDestroy Check Points: {DestroyCheckPoints}" +
                    $"\nPlayer Tag: {PlayerTag}" +
                    $"\nSave Period: {SavePeriod}"
                );
            }
        }


        /// <summary>
        /// Pass <see cref="IProgress{T}"> IProgress object </see> to observe async saving progress
        /// when it'll be started
        /// </summary>
        /// <remarks> The Core will report progress only during async save </remarks>
        public static void ObserveProgress ([NotNull] IProgress<float> progress) {
            m_progress = progress ?? throw new ArgumentNullException(nameof(progress));

            if (DebugEnabled)
                Logger.Log($"Progress observer {m_progress} was register");
        }


        /// <summary>
        /// Binds any key with quick save
        /// </summary>
        public static void BindKey (KeyCode keyCode) {
            m_quickSaveKey = keyCode;

            if (DebugEnabled)
                Logger.Log($"Key '{m_quickSaveKey}' was bind with quick save");
        }


        /// <summary>
        /// You can call it when any event was happened
        /// </summary>
        public static void QuickSave () {
            const string message = "Successful quick-save";
            ScheduleSave(SaveType.QuickSave, message, m_cancellationSource.Token);
        }


        /// <summary>
        /// Loads a scene which was saved at last session
        /// </summary>
        /// <param name="defaultSceneIndex"> If there is no saved scene, load a scene by the index </param>
        public static void LoadSavedScene (int defaultSceneIndex = 0) {
            SceneManager.LoadScene(m_lastSceneIndex != -1 ? m_lastSceneIndex : defaultSceneIndex);
        }


        /// <summary>
        /// Loads a scene which was saved at last session (async version)
        /// </summary>
        /// <param name="defaultSceneIndex"> If there is no saved scene, load a scene by the index </param>
        public static async UniTask LoadSavedSceneAsync (int defaultSceneIndex = 0) {
            await SceneManager.LoadSceneAsync(m_lastSceneIndex != -1 ? m_lastSceneIndex : defaultSceneIndex);
        }


        /// <summary>
        /// Call this to manually save handlers before loading another scene
        /// </summary>
        /// <param name="sceneIndex"> Loading scene index </param>
        /// <param name="asyncLoad"> Load scene asynchronously? </param>
        public static async UniTask SaveAndLoadScene (int sceneIndex, bool asyncLoad = false) {
            m_autoSaveEnabled = false;

            m_onSaveStart?.Invoke(SaveType.OnSwitchScene);
            SaveHandlers();
            await SaveAsyncHandlers();
            m_onSaveEnd?.Invoke(SaveType.OnSwitchScene);

            if (DebugEnabled)
                Logger.Log("Successful async saving before switch scene");

            if (asyncLoad)
                await SceneManager.LoadSceneAsync(sceneIndex);
            else
                SceneManager.LoadScene(sceneIndex);

            m_autoSaveEnabled = m_enabledSaveEvents.HasFlag(SaveEvents.AutoSave);
            m_autoSaveLastTime = Time.time;
        }


        /// <summary>
        /// Call this to manually save handlers before quitting the application
        /// </summary>
        /// <remarks>
        /// This will immediately exit the game after saving.
        /// You should make sure that you don't need to do anything else before calling it
        /// </remarks>
        public static async UniTask SaveAndQuit () {
            m_cancellationSource.Cancel();
            m_autoSaveEnabled = false;
            m_quickSaveKey = default;

            m_onSaveStart?.Invoke(SaveType.OnExit);
            SaveHandlers();
            await SaveAsyncHandlers();
            m_onSaveEnd?.Invoke(SaveType.OnExit);

            if (DebugEnabled)
                Logger.Log("Successful async saving before the quitting");

            Application.quitting -= SaveBeforeExit;

        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
        }

    }

}