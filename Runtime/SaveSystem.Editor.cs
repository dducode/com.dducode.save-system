#if UNITY_EDITOR
using SaveSystemPackage.Internal;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Logger = SaveSystemPackage.Internal.Logger;

namespace SaveSystemPackage {

    public static partial class SaveSystem {

        static partial void SetOnExitPlayModeCallback () {
            Application.quitting += ResetOnExit;
        }


        private static void ResetOnExit () {
            ResetPlayerLoop(PlayerLoop.GetCurrentPlayerLoop());

            Application.focusChanged -= OnFocusLost;
            Application.lowMemory -= OnLowMemory;

            s_synchronizationPoint.Clear();
            exitCancellation.Cancel();
            s_periodicSaveLastTime = 0;
            Logger.FlushLogs();
            if (Logger is Logger defaultLogger)
                defaultLogger.Dispose();
            Logger = null;
            Settings = null;
            Game = null;
            ProfilesManager = null;
            Initialized = false;

            Application.quitting -= ResetOnExit;
        }


        private static void ResetPlayerLoop (PlayerLoopSystem modifiedLoop) {
            if (PlayerLoopManager.TryRemoveSubSystem(ref modifiedLoop, typeof(SaveSystem), typeof(PreLateUpdate)))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError(nameof(SaveSystem), $"Failed remove system: {typeof(SaveSystem)}");
        }

    }

}
#endif