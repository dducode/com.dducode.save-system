#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.LowLevel;
using Logger = SaveSystem.Internal.Logger;

namespace SaveSystem.Core {

    public static partial class SaveSystemCore {

        static partial void ResetOnExitPlayMode (PlayerLoopSystem modifiedLoop, PlayerLoopSystem saveSystemLoop) {
            EditorApplication.playModeStateChanged += state => {
                if (state is PlayModeStateChange.EnteredEditMode) {
                    ResetPlayerLoop(modifiedLoop, saveSystemLoop);
                    EnabledSaveEvents = SaveEvents.None;
                    m_autoSaveEnabled = false;
                    SavePeriod = 0;
                    OnSaveStart = null;
                    OnSaveEnd = null;
                    Handlers.Clear();
                    AsyncHandlers.Clear();
                    m_quickSaveKey = default;
                    m_destroyedCheckpoints.Clear();
                    m_autoSaveLastTime = 0;
                }
            };
        }


        private static void ResetPlayerLoop (PlayerLoopSystem modifiedLoop, PlayerLoopSystem saveSystemLoop) {
            if (ModifyUpdateSystem(ref modifiedLoop, saveSystemLoop, ModifyType.Remove))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError("Remove system failed");
        }

    }

}
#endif