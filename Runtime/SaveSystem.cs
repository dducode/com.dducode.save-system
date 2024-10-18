#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Serialization;
using SaveSystemPackage.Settings;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    public static partial class SaveSystem {

        internal static event Action OnUpdateSystem;
        internal static readonly ISerializer EditorSerializer = new YamlSerializer();

        private static readonly SynchronizationPoint s_synchronizationPoint = new();

        /// It will be canceled before exit game
        private static CancellationTokenSource exitCancellation;

        private static bool s_periodicSaveEnabled;
        private static float s_periodicSaveLastTime;
        private static float s_logsFlushingLastTime;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static async void AutoInit () {
            SaveSystemSettings settings = SaveSystemSettings.Load();

            if (settings != null && settings.automaticInitialize) {
                settings.Dispose();
                await Initialize();
            }
        }


        private static void SetPlayerLoop () {
            PlayerLoopSystem modifiedLoop = PlayerLoop.GetCurrentPlayerLoop();
            var saveSystemLoop = new PlayerLoopSystem {
                type = typeof(SaveSystem),
                updateDelegate = UpdateSystem
            };

            if (PlayerLoopManager.TryInsertSubSystem(ref modifiedLoop, saveSystemLoop, typeof(PreLateUpdate)))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError(nameof(SaveSystem), $"Failed insert system: {saveSystemLoop}");
        }


        static partial void SetOnExitPlayModeCallback ();


        internal static void SaveAtCheckpoint (Component other) {
            if (!other.CompareTag(Settings.PlayerTag))
                return;

            ScheduleSave(SaveType.SaveAtCheckpoint);
        }


        private static void UpdateSystem () {
            if (exitCancellation.IsCancellationRequested)
                return;

            /*
             * Call saving request only in the one state machine
             * This is necessary to prevent sharing of the same file
             */
            s_synchronizationPoint.ExecuteScheduledTask(exitCancellation.Token);

            UpdateUserInputs();

            if (s_periodicSaveEnabled)
                PeriodicSave();

            if (s_logsFlushingLastTime + Settings.LogsFlushingTime < Time.time)
                Logger.FlushLogs();

            OnUpdateSystem?.Invoke();
        }


        private static void UpdateUserInputs () {
        #if ENABLE_BOTH_SYSTEMS
            switch (Settings.UsedInputSystem) {
                case UsedInputSystem.LegacyInputManager:
                    CheckPressedKeys();
                    break;
                case UsedInputSystem.InputSystem:
                    CheckPerformedActions();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        #else
        #if ENABLE_LEGACY_INPUT_MANAGER
            CheckPressedKeys();
        #endif

        #if ENABLE_INPUT_SYSTEM
            CheckPerformedActions();
        #endif
        #endif
        }


    #if ENABLE_LEGACY_INPUT_MANAGER
        private static void CheckPressedKeys () {
            if (Input.GetKeyDown(Settings.QuickSaveKey))
                QuickSave();
        }
    #endif


    #if ENABLE_INPUT_SYSTEM
        private static void CheckPerformedActions () {
            if (Settings.QuickSaveAction != null &&
                Settings.QuickSaveAction.WasPerformedThisFrame())
                QuickSave();
        }
    #endif


        private static void QuickSave () {
            ScheduleSave(SaveType.QuickSave);
        }


        private static void PeriodicSave () {
            if (s_periodicSaveLastTime + Settings.SavePeriod < Time.time) {
                ScheduleSave(SaveType.PeriodicSave);
                s_periodicSaveLastTime = Time.time;
            }
        }


        private static void SetupEvents (SaveEvents enabledSaveEvents) {
            s_periodicSaveEnabled = false;
            Application.focusChanged -= OnFocusLost;
            Application.lowMemory -= OnLowMemory;

            switch (enabledSaveEvents) {
                case SaveEvents.None:
                    break;
                case not SaveEvents.All:
                    s_periodicSaveEnabled = enabledSaveEvents.HasFlag(SaveEvents.PeriodicSave);
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnFocusLost))
                        Application.focusChanged += OnFocusLost;
                    if (enabledSaveEvents.HasFlag(SaveEvents.OnLowMemory))
                        Application.lowMemory += OnLowMemory;
                    break;
                case SaveEvents.All:
                    s_periodicSaveEnabled = true;
                    Application.focusChanged += OnFocusLost;
                    Application.lowMemory += OnLowMemory;
                    break;
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
            s_synchronizationPoint.ScheduleTask(async token => await CommonSavingTask(saveType, token));
        }


        private static async Task CommonSavingTask (SaveType saveType, CancellationToken token) {
            OnSaveStart?.Invoke(saveType);
            HandlingResult result;

            try {
                token.ThrowIfCancellationRequested();
                await Game.Save(saveType, token);
                result = HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                result = HandlingResult.Canceled;
            }
            catch (Exception exception) {
                Debug.LogException(exception);
                result = HandlingResult.Error;
            }

            OnSaveEnd?.Invoke(saveType);
            LogMessage(saveType, result);
        }


        private static void LogMessage (SaveType saveType, HandlingResult result) {
            if (result is HandlingResult.Success)
                Logger.Log(nameof(SaveSystem), $"{saveType}: success");
            else if (result is HandlingResult.Canceled)
                Logger.LogWarning(nameof(SaveSystem), $"{saveType}: canceled");
            else if (result is HandlingResult.Error)
                Logger.LogError(nameof(SaveSystem), $"{saveType}: error");
        }

    }

}