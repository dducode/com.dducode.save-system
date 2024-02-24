using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = SaveSystem.Internal.Logger;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
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

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
                        "Save period cannot be less than 0.", nameof(SavePeriod)
                    );
                }

                m_savePeriod = value;
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
                Logger.Log((m_isParallel ? "Enable" : "Disable") + " parallel saving");
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
                Logger.Log((m_destroyCheckPoints ? "Enable" : "Disable") + " destroying checkpoints");
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
                        nameof(PlayerTag), "Player tag cannot be null or empty."
                    );
                }

                m_playerTag = value;
                Logger.Log($"Set player tag: {m_playerTag}");
            }
        }

        public static LogLevel EnabledLogs {
            get => Logger.EnabledLogs;
            set {
                Logger.EnabledLogs = value;
                Logger.Log($"Set enabled logs: {Logger.EnabledLogs}");
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
                Logger.Log($"Listener {value.Method.Name} subscribe to {nameof(OnSaveStart)} event");
            }
            remove {
                m_onSaveStart -= value;
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
                Logger.Log($"Listener {value.Method.Name} subscribe to {nameof(OnSaveEnd)} event");
            }
            remove {
                m_onSaveEnd -= value;
                Logger.Log($"Listener {value.Method.Name} unsubscribe from {nameof(OnSaveEnd)} event");
            }
        }


        public static SaveProfile[] GetAllProfiles () {
            string[] paths = Directory.GetFileSystemEntries(Application.persistentDataPath, SaveProfileExtension);
            var profiles = new SaveProfile[paths.Length];

            for (var i = 0; i < profiles.Length; i++) {
                using var reader = new SaveSystem.BinaryHandlers.BinaryReader(File.Open(paths[i], FileMode.Open));
                profiles[i] = new SaveProfile();
                profiles[i].Deserialize(reader);
            }

            return profiles;
        }


        public static void SetProfile (SaveProfile saveProfile) {
            m_currentProfile = saveProfile;
        }


        /// <summary>
        /// Registers an serializable object to automatic save, quick-save, save at checkpoit and others
        /// </summary>
        public static void RegisterSerializable ([NotNull] IRuntimeSerializable serializable) {
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            SerializableObjects.Add(serializable);
            Logger.Log($"Serializable object: {serializable} was registered");
        }


        /// <summary>
        /// Registers some serializable objects to automatic save, quick-save, save at checkpoit and others
        /// </summary>
        public static void RegisterSerializables ([NotNull] IEnumerable<IRuntimeSerializable> serializables) {
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            foreach (IRuntimeSerializable serializable in serializables)
                SerializableObjects.Add(serializable);

            Logger.Log($"Serializable objects was registered");
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureParameters (
            SaveEvents enabledSaveEvents,
            bool isParallel,
            LogLevel enabledLogs,
            bool destroyCheckPoints,
            string playerTag,
            float savePeriod = 0
        ) {
            m_enabledSaveEvents = enabledSaveEvents;
            SetupEvents(m_enabledSaveEvents);
            m_isParallel = isParallel;
            Logger.EnabledLogs = enabledLogs;
            m_destroyCheckPoints = destroyCheckPoints;
            m_playerTag = playerTag;
            m_savePeriod = savePeriod;

            Logger.Log(
                "Parameters was configured:" +
                $"\nEnabled Save Events: {EnabledSaveEvents}" +
                $"\nIs Parallel: {IsParallel}" +
                $"\nEnabled Logs: {enabledLogs}" +
                $"\nDestroy Check Points: {DestroyCheckPoints}" +
                $"\nPlayer Tag: {PlayerTag}" +
                $"\nSave Period: {SavePeriod}"
            );
        }


        /// <summary>
        /// Pass <see cref="IProgress{T}"> IProgress</see> object to observe progress
        /// when it'll be started
        /// </summary>
        /// <remarks> The Core will report progress only during async save </remarks>
        public static void ObserveProgress ([NotNull] IProgress<float> progress) {
            m_saveProgress = progress ?? throw new ArgumentNullException(nameof(progress));
            m_loadProgress = progress;
            Logger.Log($"Progress observer {progress} was register");
        }


        /// <summary>
        /// Pass two <see cref="IProgress{T}"> IProgress </see> objects to observe saving and loading progress
        /// when it'll be started
        /// </summary>
        /// <remarks> The Core will report progress only during async save </remarks>
        public static void ObserveProgress (
            [NotNull] IProgress<float> saveProgress, [NotNull] IProgress<float> loadProgress
        ) {
            m_saveProgress = saveProgress ?? throw new ArgumentNullException(nameof(saveProgress));
            m_loadProgress = loadProgress ?? throw new ArgumentNullException(nameof(loadProgress));
            Logger.Log($"Progress observers {saveProgress} and {loadProgress} was registered");
        }


    #if ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Binds any key with quick save
        /// </summary>
        public static void BindKey (KeyCode keyCode) {
            m_quickSaveKey = keyCode;
            Logger.Log($"Key \"{m_quickSaveKey}\" was bind with quick save");
        }
    #endif


    #if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Binds any input action with quick save
        /// </summary>
        public static void BindAction (InputAction action) {
            m_quickSaveAction = action;
            Logger.Log($"Action \"{m_quickSaveAction}\" was bind with quick save");
        }
    #endif


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
    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public static async UniTask LoadSavedSceneAsync (int defaultSceneIndex = 0) {
            await SceneManager.LoadSceneAsync(m_lastSceneIndex != -1 ? m_lastSceneIndex : defaultSceneIndex);
        }
    #else
        public static IEnumerator LoadSavedSceneAsync (int defaultSceneIndex = 0) {
            yield return SceneManager.LoadSceneAsync(m_lastSceneIndex != -1 ? m_lastSceneIndex : defaultSceneIndex);
        }
    #endif


        /// <summary>
        /// Run loading immediately and pass any action to continue
        /// </summary>
        public static async void LoadAsync (Action<HandlingResult> continuation, CancellationToken token = default) {
            var result = HandlingResult.InternalError;

            try {
                result = await LoadAsync(token);
            }
            finally {
                continuation(result);
            }
        }


        /// <summary>
        /// Run loading immediately and wait it
        /// </summary>
        public static async TaskResult LoadAsync (CancellationToken token = default) {
            token.ThrowIfCancellationRequested();

            while (SynchronizationPoint.IsPerformed)
                await TaskAlias.Yield(token);

            LoadProfileMetadata();
            return await SynchronizationPoint.ExecuteTask(LoadObjectGroups, token);
        }


        /// <summary>
        /// Run saving immediately and pass any action to continue
        /// </summary>
        public static async void SaveAsync (Action continuation, CancellationToken token = default) {
            try {
                await SaveAsync(token);
            }
            finally {
                continuation();
            }
        }


        /// <summary>
        /// Run saving immediately and wait it
        /// </summary>
        public static async TaskAlias SaveAsync (CancellationToken token = default) {
            token.ThrowIfCancellationRequested();

            while (SynchronizationPoint.IsPerformed)
                await TaskAlias.Yield(token);

            SaveProfileMetadata();
            await SynchronizationPoint.ExecuteTask(SaveObjectGroups, token);
        }

    }

}