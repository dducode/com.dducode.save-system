using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.Internal.Templates;
using UnityEngine;
using Logger = SaveSystem.Internal.Logger;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class SaveSystemCore {

        /// <summary>
        /// Uses for serializing data into a separately file
        /// </summary>
        [NotNull]
        public static SaveProfile SelectedSaveProfile {
            get => m_selectedSaveProfile;
            set {
                if (value == null)
                    throw new ArgumentNullException(nameof(SelectedSaveProfile));

                RegisterSaveProfile(m_selectedSaveProfile = value);
                Logger.Log(nameof(SaveSystemCore), $"Set save profile: {{{value}}}");
            }
        }

        /// <summary>
        /// It's used to manage autosave loop, save on focus changed, on low memory and on quitting the game
        /// </summary>
        /// <example> EnabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnFocusChanged </example>
        /// <seealso cref="SaveEvents"/>
        public static SaveEvents EnabledSaveEvents {
            get => m_enabledSaveEvents;
            set {
                SetupEvents(m_enabledSaveEvents = value);
                Logger.Log(nameof(SaveSystemCore), $"Set save events: {value}");
            }
        }

        /// <summary>
        /// It's used to enable logs
        /// </summary>
        /// <example> EnabledLogs = LogLevel.Warning | LogLevel.Error </example>
        /// <seealso cref="LogLevel"/>
        public static LogLevel EnabledLogs {
            get => Logger.EnabledLogs;
            set {
                Logger.EnabledLogs = value;
                Logger.Log(nameof(SaveSystemCore), $"Set enabled logs: {value}");
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
                Logger.Log(nameof(SaveSystemCore), $"Set save period: {value}");
            }
        }

        /// <summary>
        /// Configure it to set parallel saving handlers
        /// </summary>
        public static bool IsParallel {
            get => m_isParallel;
            set {
                m_isParallel = value;
                Logger.Log(nameof(SaveSystemCore), (value ? "Enable" : "Disable") + " parallel saving");
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
                        nameof(PlayerTag), "Player tag cannot be null or empty."
                    );
                }

                m_playerTag = value;
                Logger.Log(nameof(SaveSystemCore), $"Set player tag: {value}");
            }
        }


        /// <summary>
        /// Set the global data path
        /// </summary>
        [NotNull]
        public static string DataPath {
            get => m_dataPath;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataPath), "Data path cannot be null or empty");

                m_dataPath = Storage.PrepareBeforeUsing(value, false);
                Logger.Log(nameof(SaveSystemCore), $"Set data path: {value}");
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
                Logger.Log(nameof(SaveSystemCore),
                    $"Listener {value.Method.Name} subscribe to {nameof(OnSaveStart)} event"
                );
            }
            remove {
                m_onSaveStart -= value;
                Logger.Log(nameof(SaveSystemCore),
                    $"Listener {value.Method.Name} unsubscribe from {nameof(OnSaveStart)} event"
                );
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
                Logger.Log(nameof(SaveSystemCore),
                    $"Listener {value.Method.Name} subscribe to {nameof(OnSaveEnd)} event"
                );
            }
            remove {
                m_onSaveEnd -= value;
                Logger.Log(nameof(SaveSystemCore),
                    $"Listener {value.Method.Name} unsubscribe from {nameof(OnSaveEnd)} event"
                );
            }
        }


        /// <summary>
        /// Buffer to writing data in a global scope
        /// </summary>
        public static DataBuffer DataBuffer {
            get {
                if (!m_loaded)
                    Logger.LogWarning(nameof(SaveSystemCore), Messages.TryingToReadNotLoadedData);
                return m_dataBuffer;
            }
        }


        /// <summary>
        /// Get all previously created saving profiles
        /// </summary>
        public static List<TProfile> LoadAllProfiles<TProfile> () where TProfile : SaveProfile, new() {
            string[] paths = Directory.GetFileSystemEntries(
                Storage.PersistentDataPath, $"*{ProfileMetadataExtension}", SearchOption.AllDirectories
            );
            var profiles = new List<TProfile>();

            foreach (string path in paths) {
                using var reader = new SaveReader(File.Open(path, FileMode.Open));
                if (typeof(TProfile).ToString() != reader.ReadString())
                    continue;

                var profile = new TProfile();
                profile.Deserialize(reader);
                profiles.Add(profile);
            }

            return profiles;
        }


        /// <summary>
        /// Saves new profile into the internal persistent storage
        /// </summary>
        public static void RegisterSaveProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string path = Path.Combine(m_profilesFolderPath, $"{profile.Name}{ProfileMetadataExtension}");
            if (File.Exists(path))
                return;

            using var writer = new SaveWriter(File.Open(path, FileMode.OpenOrCreate));
            writer.Write(profile.GetType().ToString());
            profile.Serialize(writer);
            Logger.Log(nameof(SaveSystemCore), $"Profile {{{profile}}} was registered");
        }


        /// <summary>
        /// Removes profile from the internal persistent storage
        /// </summary>
        public static void DeleteSaveProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string path = Path.Combine(m_profilesFolderPath, $"{profile.Name}{ProfileMetadataExtension}");
            if (!File.Exists(path))
                return;

            File.Delete(path);
            Directory.Delete(profile.ProfileDataFolder, true);
            Logger.Log(nameof(SaveSystemCore), $"Profile {{{profile}}} was deleted");
        }


        /// <summary>
        /// Registers an serializable object to automatic save, quick-save, save at checkpoit and others
        /// </summary>
        public static void RegisterSerializable ([NotNull] IRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(nameof(SaveSystemCore), Messages.RegistrationClosed);
                return;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            SerializableObjects.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(nameof(SaveSystemCore), $"Serializable object {serializable} was registered");
        }


        /// <summary>
        /// Registers an async serializable object to automatic save, quick-save, save at checkpoit and others
        /// </summary>
        public static void RegisterSerializable ([NotNull] IAsyncRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(nameof(SaveSystemCore), Messages.RegistrationClosed);
                return;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            AsyncSerializableObjects.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(nameof(SaveSystemCore), $"Serializable object {serializable} was registered");
        }


        /// <summary>
        /// Registers some serializable objects to automatic save, quick-save, save at checkpoit and others
        /// </summary>
        public static void RegisterSerializables ([NotNull] IEnumerable<IRuntimeSerializable> serializables) {
            if (m_registrationClosed) {
                Logger.LogError(nameof(SaveSystemCore), Messages.RegistrationClosed);
                return;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();
            SerializableObjects.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log(nameof(SaveSystemCore), "Serializable objects was registered");
        }


        /// <summary>
        /// Registers some async serializable objects to automatic save, quick-save, save at checkpoit and others
        /// </summary>
        public static void RegisterSerializables ([NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables) {
            if (m_registrationClosed) {
                Logger.LogError(nameof(SaveSystemCore), Messages.RegistrationClosed);
                return;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IAsyncRuntimeSerializable[] objects =
                serializables as IAsyncRuntimeSerializable[] ?? serializables.ToArray();
            AsyncSerializableObjects.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log(nameof(SaveSystemCore), "Serializable objects was registered");
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureParameters (
            SaveEvents enabledSaveEvents,
            LogLevel enabledLogs,
            bool isParallel,
            string playerTag,
            float savePeriod = 0
        ) {
            SetupEvents(m_enabledSaveEvents = enabledSaveEvents);
            Logger.EnabledLogs = enabledLogs;
            m_isParallel = isParallel;
            m_playerTag = playerTag;
            m_savePeriod = savePeriod;

            Logger.Log(nameof(SaveSystemCore),
                "Parameters was configured:" +
                $"\nEnabled Save Events: {EnabledSaveEvents}" +
                $"\nIs Parallel: {IsParallel}" +
                $"\nEnabled Logs: {enabledLogs}" +
                $"\nPlayer Tag: {PlayerTag}" +
                $"\nSave Period: {SavePeriod}"
            );
        }


        /// <summary>
        /// Pass <see cref="IProgress{T}"> IProgress</see> object to observe progress
        /// when it'll be started
        /// </summary>
        public static void ObserveProgress ([NotNull] IProgress<float> progress) {
            m_saveProgress = progress ?? throw new ArgumentNullException(nameof(progress));
            m_loadProgress = progress;
            Logger.Log(nameof(SaveSystemCore), $"Progress observer {progress} was register");
        }


        /// <summary>
        /// Pass two <see cref="IProgress{T}"> IProgress </see> objects to observe saving and loading progress
        /// when it'll be started
        /// </summary>
        public static void ObserveProgress (
            [NotNull] IProgress<float> saveProgress, [NotNull] IProgress<float> loadProgress
        ) {
            m_saveProgress = saveProgress ?? throw new ArgumentNullException(nameof(saveProgress));
            m_loadProgress = loadProgress ?? throw new ArgumentNullException(nameof(loadProgress));
            Logger.Log(nameof(SaveSystemCore), $"Progress observers {saveProgress} and {loadProgress} was registered");
        }


    #if ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Binds any key with quick save
        /// </summary>
        public static void BindKey (KeyCode keyCode) {
            m_quickSaveKey = keyCode;
            Logger.Log(nameof(SaveSystemCore), $"Key \"{keyCode}\" was bind with quick save");
        }
    #endif


    #if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Binds any input action with quick save
        /// </summary>
        public static void BindAction (InputAction action) {
            m_quickSaveAction = action;
            Logger.Log(nameof(SaveSystemCore), $"Action \"{action}\" was bind with quick save");
        }
    #endif


        public static async UniTask LoadSceneAsync (Action sceneLoading, CancellationToken token = default) {
            if (m_enabledSaveEvents.HasFlag(SaveEvents.OnSceneLoad))
                await ExecuteOnSceneLoadSaving(token);
            await SceneLoader.LoadSceneAsync(sceneLoading);
        }


        public static async UniTask LoadSceneAsync<TData> (
            Action sceneLoading, TData passedData, CancellationToken token = default
        ) {
            if (m_enabledSaveEvents.HasFlag(SaveEvents.OnSceneLoad))
                await ExecuteOnSceneLoadSaving(token);
            await SceneLoader.LoadSceneAsync(sceneLoading, passedData);
        }


        public static async UniTask LoadSceneAsync (
            Func<UniTask> asyncSceneLoading, CancellationToken token = default
        ) {
            if (m_enabledSaveEvents.HasFlag(SaveEvents.OnSceneLoad))
                await ExecuteOnSceneLoadSaving(token);
            await SceneLoader.LoadSceneAsync(asyncSceneLoading);
        }


        public static async UniTask LoadSceneAsync<TData> (
            Func<UniTask> asyncSceneLoading, TData passedData, CancellationToken token = default
        ) {
            if (m_enabledSaveEvents.HasFlag(SaveEvents.OnSceneLoad))
                await ExecuteOnSceneLoadSaving(token);
            await SceneLoader.LoadSceneAsync(asyncSceneLoading, passedData);
        }


        /// <summary>
        /// Start saving immediately and pass any action to continue
        /// </summary>
        public static async void SaveAsync (Action<HandlingResult> continuation, CancellationToken token = default) {
            try {
                continuation(await SaveAsync(token));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        /// <summary>
        /// Start saving immediately and wait it
        /// </summary>
        public static async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                return await SynchronizationPoint.ExecuteTask(async () => await SaveObjectsAsync(token));
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        /// <summary>
        /// Start loading of objects in the project scope and pass any action to continue
        /// </summary>
        public static async void LoadAsync (Action<HandlingResult> continuation, CancellationToken token = default) {
            try {
                continuation(await LoadAsync(token));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        /// <summary>
        /// Start loading of objects in the project scope and wait it
        /// </summary>
        public static async UniTask<HandlingResult> LoadAsync (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                return await SynchronizationPoint.ExecuteTask(async () => await LoadGlobalDataAsync(token));
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        private static async UniTask ExecuteOnSceneLoadSaving (CancellationToken token) {
            await SynchronizationPoint.ExecuteTask(async () => await CommonSavingTask(SaveType.OnSceneLoad, token));
        }

    }

}