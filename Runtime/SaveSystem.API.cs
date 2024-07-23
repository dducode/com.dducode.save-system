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
using SaveSystemPackage.Internal.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = SaveSystemPackage.Internal.Logger;

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
            Game = new Game();

            SetPlayerLoop();
            SetSettings(ResourcesManager.LoadSettings());
            SetInternalFolder();
            SetOnExitPlayModeCallback();

            m_exitCancellation = new CancellationTokenSource();
            Logger.Log(nameof(SaveSystem), "Initialized");
        }


        /// <summary>
        /// Creates new save profile and stores it in the internal storage
        /// </summary>
        public static SaveProfile CreateProfile (
            [NotNull] string name, bool autoSave = true, bool encrypt = true, bool authenticate = true
        ) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            string path = Path.Combine(InternalFolder, $"{name.ToPathFormat()}.profilemetadata");
            var profile = new SaveProfile(name, autoSave, encrypt, authenticate);
            using var writer = new SaveWriter(File.Open(path, FileMode.OpenOrCreate));
            writer.Write(name);
            writer.Write(autoSave);
            writer.Write(encrypt);
            writer.Write(authenticate);
            return profile;
        }


        /// <summary>
        /// Get all previously created saving profiles
        /// </summary>
        [Pure]
        public static async IAsyncEnumerable<SaveProfile> LoadAllProfiles () {
            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profilemetadata");

            foreach (string path in paths) {
                await using var reader = new SaveReader(File.Open(path, FileMode.Open));
                var profile = new SaveProfile(
                    reader.ReadString(), reader.Read<bool>(), reader.Read<bool>(), reader.Read<bool>()
                );
                await profile.Load();
                yield return profile;
            }
        }


        /// <summary>
        /// Get saving profile by its name
        /// </summary>
        [Pure]
        public static async UniTask<SaveProfile> LoadProfile ([NotNull] string name) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profilemetadata");

            foreach (string path in paths) {
                await using var reader = new SaveReader(File.Open(path, FileMode.Open));

                if (string.Equals(reader.ReadString(), name)) {
                    var profile = new SaveProfile(
                        name, reader.Read<bool>(), reader.Read<bool>(), reader.Read<bool>()
                    );
                    await profile.Load();
                    return profile;
                }
            }

            return null;
        }


        /// <summary>
        /// Removes profile from the internal persistent storage
        /// </summary>
        public static void DeleteSaveProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string path = Path.Combine(InternalFolder, $"{profile.Name}.profilemetadata");
            if (!File.Exists(path))
                return;

            File.Delete(path);
            Directory.Delete(profile.DataFolder, true);
            Logger.Log(nameof(SaveSystem), $"Profile {{{profile}}} deleted");
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