using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal;
using SaveSystem.Internal.Extensions;
using SaveSystem.Internal.Templates;
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
        private static string m_dataPath;

        private static SaveProfile m_selectedSaveProfile;
        private static SaveEvents m_enabledSaveEvents;
        private static float m_savePeriod;
        private static bool m_isParallel;
        private static string m_playerTag;

        private static Action<SaveType> m_onSaveStart;
        private static Action<SaveType> m_onSaveEnd;

        /// It will be canceled before exit game
        private static CancellationTokenSource m_exitCancellation;

        private static DataBuffer m_dataBuffer = new();

        private static readonly List<IRuntimeSerializable> SerializableObjects = new();
        private static readonly List<IAsyncRuntimeSerializable> AsyncSerializableObjects = new();
        private static int ObjectsCount => SerializableObjects.Count + AsyncSerializableObjects.Count;

    #if ENABLE_LEGACY_INPUT_MANAGER
        private static KeyCode m_quickSaveKey;
    #endif

    #if ENABLE_INPUT_SYSTEM
        private static InputAction m_quickSaveAction;
    #endif

        private static bool m_autoSaveEnabled;
        private static float m_autoSaveLastTime;
        private static IProgress<float> m_saveProgress;
        private static IProgress<float> m_loadProgress;
        private static readonly SynchronizationPoint SynchronizationPoint = new();
        private static bool m_savedBeforeExit;
        private static bool m_loaded;
        private static bool m_registrationClosed;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize () {
            SetPlayerLoop();
            SetSettings();
            SetInternalSavedPaths();
            SetOnExitPlayModeCallback();

            m_selectedSaveProfile = SaveProfile.Default;
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


        private static void SetSettings () {
            var settings = Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings));

            if (settings == null) {
                settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
                Debug.LogWarning(Logger.FormattedMessage(
                    nameof(SaveSystemCore), Messages.SettingsNotFound
                ));
            }

            // Core settings
            SetupEvents(m_enabledSaveEvents = settings.enabledSaveEvents);
            m_dataPath = Storage.PrepareBeforeUsing(settings.dataPath, false);
            m_savePeriod = settings.savePeriod;
            m_isParallel = settings.isParallel;
            Logger.EnabledLogs = settings.enabledLogs;

            // Checkpoints settings
            m_playerTag = settings.playerTag;
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
                m_onSaveStart?.Invoke(SaveType.OnExit);
                SaveObjectsAsync(OnSaveEnd);
                return false;
            }

            void OnSaveEnd (HandlingResult result) {
                m_onSaveEnd?.Invoke(SaveType.OnExit);
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
            m_onSaveStart?.Invoke(saveType);
            HandlingResult result = await SaveObjectsAsync(token);
            m_onSaveEnd?.Invoke(saveType);

            if (result is HandlingResult.Success)
                Logger.Log(nameof(SaveSystemCore), $"{saveType}: success");
            else if (result is HandlingResult.Canceled)
                Logger.LogWarning(nameof(SaveSystemCore), $"{saveType}: canceled");
            else if (result is HandlingResult.Error)
                Logger.LogError(nameof(SaveSystemCore), $"{saveType}: error");

            return result;
        }


        private static async void SaveObjectsAsync (Action<HandlingResult> continuation) {
            try {
                continuation(await SaveObjectsAsync(CancellationToken.None));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        private static async UniTask<HandlingResult> SaveObjectsAsync (CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                await SaveGlobalDataAsync(token);
                await m_selectedSaveProfile.SaveProfileDataAsync(token);

                SceneSerializationContext[] contexts =
                    Object.FindObjectsByType<SceneSerializationContext>(FindObjectsSortMode.None);

                if (contexts.Length > 0) {
                    if (IsParallel && contexts.Length > 1) {
                        await ParallelLoop.ForEachAsync(
                            contexts,
                            async sceneLoader => await sceneLoader.SaveSceneDataAsync(token)
                        );
                    }
                    else {
                        foreach (SceneSerializationContext sceneLoader in contexts)
                            await sceneLoader.SaveSceneDataAsync(token);
                    }
                }

                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Saving operation canceled");
                return HandlingResult.Canceled;
            }
        }


        private static async UniTask SaveGlobalDataAsync (CancellationToken token) {
            if (ObjectsCount == 0 && m_dataBuffer.Count == 0)
                return;
            if (!m_loaded)
                Logger.LogWarning(nameof(SaveSystemCore), "Start saving when objects not loaded");

            m_registrationClosed = true;

            token.ThrowIfCancellationRequested();
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);

            writer.Write(m_dataBuffer);
            await SerializeObjects(writer, token);

            await memoryStream.WriteDataToFileAsync(m_dataPath, token);
            Logger.Log(nameof(SaveSystemCore), "Saved");
        }


        private static async UniTask<HandlingResult> LoadGlobalDataAsync (CancellationToken token) {
            if (m_loaded) {
                Logger.LogWarning(nameof(SaveSystemCore), "All objects already loaded");
                return HandlingResult.Canceled;
            }

            if (!File.Exists(m_dataPath)) {
                m_registrationClosed = true;
                SetDefaults();
                m_loaded = true;
                return HandlingResult.FileNotExists;
            }

            m_registrationClosed = true;

            try {
                return await TryLoadGlobalDataAsync(token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystemCore), "Loading operation canceled");
                return HandlingResult.Canceled;
            }
        }


        private static async UniTask<HandlingResult> TryLoadGlobalDataAsync (CancellationToken token) {
            token.ThrowIfCancellationRequested();
            var memoryStream = new MemoryStream();
            await memoryStream.ReadDataFromFileAsync(m_dataPath, token);
            await using var reader = new SaveReader(memoryStream);

            m_dataBuffer = reader.ReadDataBuffer();
            await DeserializeObjects(reader, token);

            Logger.Log(nameof(SaveSystemCore), "Loaded");
            m_loaded = true;
            return HandlingResult.Success;
        }


        private static async UniTask SerializeObjects (SaveWriter writer, CancellationToken token) {
            var completedTasks = 0;

            foreach (IRuntimeSerializable obj in SerializableObjects) {
                obj.Serialize(writer);
                ReportProgress(ref completedTasks, ObjectsCount, m_saveProgress);
            }

            foreach (IAsyncRuntimeSerializable obj in AsyncSerializableObjects) {
                await obj.Serialize(writer, token);
                ReportProgress(ref completedTasks, ObjectsCount, m_saveProgress);
            }
        }


        private static async UniTask DeserializeObjects (SaveReader reader, CancellationToken token) {
            var completedTasks = 0;

            foreach (IRuntimeSerializable serializable in SerializableObjects) {
                serializable.Deserialize(reader);
                ReportProgress(ref completedTasks, ObjectsCount, m_loadProgress);
            }

            foreach (IAsyncRuntimeSerializable serializable in AsyncSerializableObjects) {
                await serializable.Deserialize(reader, token);
                ReportProgress(ref completedTasks, ObjectsCount, m_loadProgress);
            }
        }


        private static void SetDefaults () {
            IEnumerable<IDefault> serializables =
                SerializableObjects.Select(serializable => serializable as IDefault);
            foreach (IDefault serializable in serializables)
                serializable?.SetDefaults();

            IEnumerable<IDefault> asyncSerializables =
                AsyncSerializableObjects.Select(serializable => serializable as IDefault);
            foreach (IDefault serializable in asyncSerializables)
                serializable?.SetDefaults();
        }


        private static void ReportProgress (ref int completedTasks, int tasksCount, IProgress<float> progress) {
            progress?.Report((float)++completedTasks / tasksCount);
        }

    }

}