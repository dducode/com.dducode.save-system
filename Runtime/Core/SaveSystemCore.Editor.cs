#if UNITY_EDITOR
using SaveSystem.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Logger = SaveSystem.Internal.Logger;

namespace SaveSystem.Core {

    public static partial class SaveSystemCore {

        static partial void ResetOnExitPlayMode () {
            EditorApplication.playModeStateChanged += state => {
                if (state is PlayModeStateChange.EnteredEditMode) {
                    ResetPlayerLoop(PlayerLoop.GetCurrentPlayerLoop());
                    ResetProperties();
                    m_onSaveStart = null;
                    m_onSaveEnd = null;
                    Handlers.Clear();
                    AsyncHandlers.Clear();
                    m_quickSaveKey = default;
                    m_destroyedCheckpoints.Clear();
                    m_autoSaveLastTime = 0;
                }
            };
        }


        private static void ResetProperties () {
            var settings = Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings));
            m_enabledSaveEvents = settings.enabledSaveEvents;
            m_savePeriod = settings.savePeriod;
            m_isParallel = settings.isParallel;
            m_debugEnabled = settings.debugEnabled;
            m_destroyCheckPoints = settings.destroyCheckPoints;
            m_playerTag = settings.playerTag;
        }


        private static void ResetPlayerLoop (PlayerLoopSystem modifiedLoop) {
            if (PlayerLoopManager.TryRemoveSubSystem(ref modifiedLoop, typeof(SaveSystemCore), typeof(PreLateUpdate)))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError($"Failed remove system: {typeof(SaveSystemCore)}");
        }

    }

}
#endif