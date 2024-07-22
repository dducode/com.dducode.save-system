﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Security;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class SaveSystem {

        /// <summary>
        /// Uses for serializing data into a separately file
        /// </summary>
        [NotNull]
        public static SaveProfile SelectedSaveProfile {
            get => m_selectedSaveProfile;
            set {
                m_selectedSaveProfile = value ?? throw new ArgumentNullException(nameof(SelectedSaveProfile));
                m_globalScope.AttachNestedScope(m_selectedSaveProfile.ProfileScope);
                if (m_sceneContext != null)
                    m_selectedSaveProfile.SceneContext = m_sceneContext;
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
        /// Set the global data path
        /// </summary>
        [NotNull]
        public static string DataPath {
            get => m_globalScope.DataPath;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataPath), "Data path cannot be null or empty");

                m_globalScope.DataPath = Storage.PrepareBeforeUsing(value);
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
            get => m_globalScope.Encrypt;
            set => m_globalScope.Encrypt = value;
        }

        /// <summary>
        /// Cryptographer used to encrypt/decrypt serializable data
        /// </summary>
        [NotNull]
        public static Cryptographer Cryptographer {
            get => m_globalScope.Cryptographer;
            set => m_globalScope.Cryptographer = value;
        }

        public static bool Authenticate {
            get => m_globalScope.Authenticate;
            set => m_globalScope.Authenticate = value;
        }

        [NotNull]
        public static AuthenticationManager AuthManager {
            get => m_globalScope.AuthManager;
            set => m_globalScope.AuthManager = value;
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


        public static void Initialize () {
            m_globalScope = new SerializationScope {
                Name = "Global scope"
            };

            SetPlayerLoop();
            SetSettings(ResourcesManager.LoadSettings());
            SetInternalFolder();
            SetOnExitPlayModeCallback();

            m_exitCancellation = new CancellationTokenSource();
            Logger.Log(nameof(SaveSystem), "Initialized");
        }


        /// <summary>
        /// Get all previously created saving profiles
        /// </summary>
        [Pure]
        public static IEnumerable<TProfile> LoadAllProfiles<TProfile> ()
            where TProfile : SaveProfile, new() {
            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profile");

            foreach (string path in paths) {
                using var reader = new SaveReader(File.Open(path, FileMode.Open));
                if (reader.ReadString() != typeof(TProfile).AssemblyQualifiedName)
                    continue;

                var profile = new TProfile();
                profile.Deserialize(reader, reader.Read<int>());
                yield return profile;
            }
        }


        /// <summary>
        /// Saves new profile into the internal persistent storage
        /// </summary>
        public static void RegisterSaveProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string path = Path.Combine(InternalFolder, $"{profile.Name}.profile");
            if (File.Exists(path))
                return;

            var memoryStream = new MemoryStream();
            using var writer = new SaveWriter(memoryStream);
            writer.Write(profile.GetType().AssemblyQualifiedName);
            writer.Write(profile.Version);
            profile.Serialize(writer);
            byte[] data = memoryStream.ToArray();
            SynchronizationPoint.ScheduleTask(
                async token => {
                    await File.WriteAllBytesAsync(path, data, token).AsUniTask();
                    Logger.Log(nameof(SaveSystem), $"Profile {{{profile}}} registered");
                },
                true
            );
        }


        /// <summary>
        /// Removes profile from the internal persistent storage
        /// </summary>
        public static void DeleteSaveProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string path = Path.Combine(InternalFolder, $"{profile.Name}.profile");
            if (!File.Exists(path))
                return;

            File.Delete(path);
            Directory.Delete(profile.DataFolder, true);
            Logger.Log(nameof(SaveSystem), $"Profile {{{profile}}} deleted");
        }


        public static void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_globalScope.WriteData(key, value);
        }


        [Pure]
        public static TValue ReadData<TValue> (
            [NotNull] string key, TValue defaultValue = default
        ) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_globalScope.ReadData(key, defaultValue);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable"/>
        public static void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            m_globalScope.RegisterSerializable(key, serializable);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables"/>
        public static void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            m_globalScope.RegisterSerializables(key, serializables);
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureSettings (SaveSystemSettings settings) {
            SetSettings(settings);
            Logger.Log(nameof(SaveSystem), $"Parameters was configured: {settings}");
        }


    #if ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Binds any key with quick save
        /// </summary>
        public static void BindKey (KeyCode keyCode) {
            m_quickSaveKey = keyCode;
            Logger.Log(nameof(SaveSystem), $"Key \"{keyCode}\" was bind with quick save");
        }
    #endif


    #if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Binds any input action with quick save
        /// </summary>
        public static void BindAction (InputAction action) {
            m_quickSaveAction = action;
            Logger.Log(nameof(SaveSystem), $"Action \"{action}\" was bind with quick save");
        }
    #endif


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


        public static async UniTask<HandlingResult> PushToCloud (
            [NotNull] ICloudStorage cloudStorage, CancellationToken token = default
        ) {
            if (cloudStorage == null)
                throw new ArgumentNullException(nameof(cloudStorage));

            try {
                token.ThrowIfCancellationRequested();
                await SynchronizationPoint.ExecuteTask(async () => await PushToCloudStorage(cloudStorage, token));
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystem), "Push to cloud canceled");
                return HandlingResult.Canceled;
            }
        }


        public static async UniTask<HandlingResult> PullFromCloud (
            [NotNull] ICloudStorage cloudStorage, CancellationToken token = default
        ) {
            if (cloudStorage == null)
                throw new ArgumentNullException(nameof(cloudStorage));

            try {
                token.ThrowIfCancellationRequested();
                await SynchronizationPoint.ExecuteTask(async () => await PullFromCloudStorage(cloudStorage, token));
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystem), "Pull from cloud canceled");
                return HandlingResult.Canceled;
            }
        }


        /// <summary>
        /// Start saving immediately and wait it
        /// </summary>
        public static async UniTask<HandlingResult> Save (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                return await SynchronizationPoint.ExecuteTask(async () => await SaveData(token));
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        /// <summary>
        /// Start loading and wait it
        /// </summary>
        public static async UniTask<HandlingResult> Load (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                return await SynchronizationPoint.ExecuteTask(async () => await LoadData(token));
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }

    }

}