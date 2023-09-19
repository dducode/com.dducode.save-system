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
                    SavePeriod = 0;
                    AutoSaveEnabled = false;
                    OnSaveEnd = null;
                    OnSaveStart = null;
                    m_lastTimeSaving = 0;
                }
            };
        }


        private static void ResetPlayerLoop (PlayerLoopSystem modifiedLoop, PlayerLoopSystem saveSystemLoop) {
            if (ModifyUpdateSystem(ref modifiedLoop, saveSystemLoop, ModifyType.Remove))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                InternalLogger.LogError("Remove system failed");
        }

    }

}
#endif