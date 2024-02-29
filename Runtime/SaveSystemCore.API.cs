﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.Tasks;
using UnityEngine;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Logger = SaveSystem.Internal.Logger;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class SaveSystemCore {

        public static SaveProfile SelectedSaveProfile {
            get => m_selectedSaveProfile;
            set {
                RegisterSaveProfile(m_selectedSaveProfile = value);
                Logger.Log($"Set save profile: {{{value}}}");
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
                Logger.Log($"Set save events: {value}");
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
                Logger.Log($"Set enabled logs: {value}");
            }
        }

        /// <summary>
        /// TODO: add description
        /// </summary>
        public static bool AllowSceneSaving {
            get => m_allowSceneSaving;
            set {
                m_allowSceneSaving = value;
                Logger.Log((value ? "Allow" : "Disallow") + " scene saving");
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
                Logger.Log($"Set save period: {value}");
            }
        }

        /// <summary>
        /// Configure it to set parallel saving handlers
        /// </summary>
        public static bool IsParallel {
            get => m_isParallel;
            set {
                m_isParallel = value;
                Logger.Log((value ? "Enable" : "Disable") + " parallel saving");
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
                Logger.Log((value ? "Enable" : "Disable") + " destroying checkpoints");
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
                Logger.Log($"Set player tag: {value}");
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


        public static TProfile[] LoadAllProfiles<TProfile> () where TProfile : SaveProfile, new() {
            string[] paths = Directory.GetFileSystemEntries(
                Storage.PersistentDataPath, $"*{ProfileExtension}", SearchOption.AllDirectories
            );
            var profiles = new TProfile[paths.Length];

            for (var i = 0; i < paths.Length; i++) {
                using var reader = new SaveSystem.BinaryHandlers.BinaryReader(File.Open(paths[i], FileMode.Open));
                var profile = new TProfile();
                profile.Deserialize(reader);
                profiles[i] = profile;
            }

            return profiles;
        }


        public static void RegisterSaveProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string path = Path.Combine(m_profilesFolderPath, $"{profile.Name}{ProfileExtension}");
            if (File.Exists(path))
                return;

            using var writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
            profile.Serialize(writer);
            Logger.Log($"Profile {{{profile}}} was registered");
        }


        public static void DeleteSaveProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string path = Path.Combine(m_profilesFolderPath, $"{profile.Name}{ProfileExtension}");
            if (!File.Exists(path))
                return;

            File.Delete(path);
            File.Delete(profile.DataPath);
            Logger.Log($"Profile {{{profile}}} was deleted");
        }


        /// <summary>
        /// Registers an serializable object to automatic save, quick-save, save at checkpoit and others
        /// </summary>
        public static void RegisterSerializable (
            [NotNull] IRuntimeSerializable serializable, [CallerMemberName] string caller = ""
        ) {
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            SerializableObjects.Add(serializable);
            DiagnosticService.AddObject(serializable, caller);
            Logger.Log($"Serializable object: {serializable} was registered");
        }


        /// <summary>
        /// Registers some serializable objects to automatic save, quick-save, save at checkpoit and others
        /// </summary>
        public static void RegisterSerializables (
            [NotNull] IEnumerable<IRuntimeSerializable> serializables, [CallerMemberName] string caller = ""
        ) {
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] array = serializables.ToArray();
            foreach (IRuntimeSerializable serializable in array)
                SerializableObjects.Add(serializable);

            DiagnosticService.AddObjects(array, caller);
            Logger.Log("Serializable objects was registered");
        }


        /// <summary>
        /// Configures all the Core parameters
        /// </summary>
        /// <remarks> You can skip it if you have configured the settings in the editor </remarks>
        public static void ConfigureParameters (
            SaveEvents enabledSaveEvents,
            LogLevel enabledLogs,
            bool isParallel,
            bool destroyCheckPoints,
            string playerTag,
            float savePeriod = 0
        ) {
            SetupEvents(m_enabledSaveEvents = enabledSaveEvents);
            Logger.EnabledLogs = enabledLogs;
            m_isParallel = isParallel;
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
            Logger.Log($"Key \"{keyCode}\" was bind with quick save");
        }
    #endif


    #if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Binds any input action with quick save
        /// </summary>
        public static void BindAction (InputAction action) {
            m_quickSaveAction = action;
            Logger.Log($"Action \"{action}\" was bind with quick save");
        }
    #endif


        /// <summary>
        /// Create a loading task while start game and configure it
        /// </summary>
        /// <seealso cref="LoadingTask"/>
        public static LoadingTask CreateLoadingTask () {
            return new LoadingTask(SerializableObjects, m_selectedSaveProfile.DataPath, m_allowSceneSaving);
        }


        /// <summary>
        /// Run saving immediately and pass any action to continue
        /// </summary>
        public static async void SaveAsync (
            Action<HandlingResult> continuation, CancellationToken token = default
        ) {
            try {
                continuation(await SaveAsync(token));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        /// <summary>
        /// Run saving immediately and wait it
        /// </summary>
        public static async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();

                while (SynchronizationPoint.IsPerformed)
                    await UniTask.Yield(token);

                return await SynchronizationPoint.ExecuteTask(async () => await SaveObjects(token));
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }

    }

}