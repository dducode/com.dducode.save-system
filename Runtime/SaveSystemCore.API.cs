using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Cryptography;
using SaveSystem.Internal;
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
            }
        }

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
            }
        }

        /// <summary>
        /// Configure it to set parallel saving handlers
        /// </summary>
        public static bool IsParallel { get; set; }

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

        public static bool Encrypt {
            get => m_handler.Encrypt;
            set => m_handler.Encrypt = value;
        }

        /// <summary>
        /// Cryptographer used to encrypt/decrypt serializable data
        /// </summary>
        [NotNull]
        public static Cryptographer Cryptographer {
            get => m_handler.Cryptographer;
            set => m_handler.Cryptographer = value;
        }

        public static bool Authentication {
            get => m_handler.Authentication;
            set => m_handler.Authentication = value;
        }

        public static HashAlgorithmName AlgorithmName {
            get => m_handler.AlgorithmName;
            set => m_handler.AlgorithmName = value;
        }

        /// <summary>
        /// Event that is called before saving. It can be useful when you use async saving
        /// </summary>
        /// <value>
        /// Listeners that will be called when core will start saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveStart;

        /// <summary>
        /// Event that is called after saving
        /// </summary>
        /// <value>
        /// Listeners that will be called when core will finish saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveEnd;


        /// <summary>
        /// Get all previously created saving profiles
        /// </summary>
        [Pure]
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
            Directory.Delete(profile.DataFolder, true);
            Logger.Log(nameof(SaveSystemCore), $"Profile {{{profile}}} was deleted");
        }


        public static void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_globalScope.WriteData(key, value);
        }


        [Pure]
        public static TValue ReadData<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_globalScope.ReadData<TValue>(key);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IRuntimeSerializable)"/>
        public static void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            m_globalScope.RegisterSerializable(key, serializable);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IAsyncRuntimeSerializable)"/>
        public static void RegisterSerializable (
            [NotNull] string key, [NotNull] IAsyncRuntimeSerializable serializable
        ) {
            m_globalScope.RegisterSerializable(key, serializable);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IRuntimeSerializable})"/>
        public static void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            m_globalScope.RegisterSerializables(key, serializables);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IAsyncRuntimeSerializable})"/>
        public static void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables
        ) {
            m_globalScope.RegisterSerializables(key, serializables);
        }


        /// <inheritdoc cref="SerializationScope.ObserveProgress(IProgress{float})"/>
        public static void ObserveProgress ([NotNull] IProgress<float> progress) {
            m_globalScope.ObserveProgress(progress);
        }


        /// <inheritdoc cref="SerializationScope.ObserveProgress(IProgress{float}, IProgress{float})"/>
        public static void ObserveProgress (
            [NotNull] IProgress<float> saveProgress, [NotNull] IProgress<float> loadProgress
        ) {
            m_globalScope.ObserveProgress(saveProgress, loadProgress);
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureSettings (SaveSystemSettings settings) {
            SetSettings(settings);
            Logger.Log(nameof(SaveSystemCore), $"Parameters was configured: {settings}");
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
        /// Start saving immediately and wait it
        /// </summary>
        public static async UniTask<HandlingResult> SaveAll (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                return await SynchronizationPoint.ExecuteTask(async () => await SaveObjects(token));
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        public static async UniTask<HandlingResult> SaveGlobalData (
            [NotNull] string dataPath, CancellationToken token = default
        ) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.SaveData(dataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Global data saving canceled");
                return HandlingResult.Canceled;
            }
        }


        [Pure]
        public static async UniTask<(HandlingResult, byte[])> SaveGlobalData (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.SaveData(token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Global data saving canceled");
                return (HandlingResult.Canceled, Array.Empty<byte>());
            }
        }


        /// <summary>
        /// Start loading of objects in the global scope and wait it
        /// </summary>
        public static async UniTask<HandlingResult> LoadGlobalData (
            [NotNull] byte[] data, CancellationToken token = default
        ) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.LoadData(data, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Global data loading canceled");
                return HandlingResult.Canceled;
            }
        }


        /// <summary>
        /// Start loading of objects in the global scope and wait it
        /// </summary>
        public static async UniTask<HandlingResult> LoadGlobalData (
            string dataPath = null, CancellationToken token = default
        ) {
            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.LoadData(dataPath ?? DataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Global data loading canceled");
                return HandlingResult.Canceled;
            }
        }

    }

}