#if UNITY_EDITOR
using SaveSystem.Internal;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Logger = SaveSystem.Internal.Logger;

namespace SaveSystem {

    public static partial class SaveSystemCore {

        static partial void SetOnExitPlayModeCallback () {
            Application.quitting += ResetOnExit;
        }


        private static void ResetOnExit () {
            ResetPlayerLoop(PlayerLoop.GetCurrentPlayerLoop());
            ResetProperties();
            m_onSaveStart = null;
            m_onSaveEnd = null;
            SerializableObjects.Clear();

        #if ENABLE_LEGACY_INPUT_MANAGER
            m_quickSaveKey = default;
        #endif

        #if ENABLE_INPUT_SYSTEM
            m_quickSaveAction = null;
        #endif

            m_destroyedCheckpoints.Clear();
            m_autoSaveLastTime = 0;
            m_savedBeforeExit = false;

            Application.quitting -= ResetOnExit;
        }


        private static void ResetPlayerLoop (PlayerLoopSystem modifiedLoop) {
            if (PlayerLoopManager.TryRemoveSubSystem(ref modifiedLoop, typeof(SaveSystemCore), typeof(PreLateUpdate)))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError($"Failed remove system: {typeof(SaveSystemCore)}");
        }


        private static void ResetProperties () {
            var settings = Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings));
            m_enabledSaveEvents = settings.enabledSaveEvents;
            m_savePeriod = settings.savePeriod;
            m_isParallel = settings.isParallel;
            Logger.EnabledLogs = settings.enabledLogs;
            m_destroyCheckPoints = settings.destroyCheckPoints;
            m_playerTag = settings.playerTag;
        }

    }

}
#endif