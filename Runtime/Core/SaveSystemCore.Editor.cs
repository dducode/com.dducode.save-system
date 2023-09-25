#if UNITY_EDITOR
using SaveSystem.InternalServices;
using UnityEditor;
using UnityEngine.LowLevel;

namespace SaveSystem.Core {

    public static partial class SaveSystemCore {

        static partial void ResetOnExitPlayMode (PlayerLoopSystem modifiedLoop, PlayerLoopSystem saveSystemLoop) {
            EditorApplication.playModeStateChanged += state => {
                if (state is PlayModeStateChange.ExitingPlayMode) {
                    ResetPlayerLoop(modifiedLoop, saveSystemLoop);
                    AutoSaveEnabled = false;
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