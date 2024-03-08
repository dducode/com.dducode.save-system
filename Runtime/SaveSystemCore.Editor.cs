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
                OnSaveStart = null;
                OnSaveEnd = null;

            #if ENABLE_LEGACY_INPUT_MANAGER
                m_quickSaveKey = default;
            #endif

            #if ENABLE_INPUT_SYSTEM
                m_quickSaveAction = null;
            #endif

                m_autoSaveLastTime = 0;
                m_savedBeforeExit = false;
                m_globalScope = null;

                Application.quitting -= ResetOnExit;
            }


            private static void ResetPlayerLoop (PlayerLoopSystem modifiedLoop) {
                if (PlayerLoopManager.TryRemoveSubSystem(ref modifiedLoop, typeof(SaveSystemCore), typeof(PreLateUpdate)))
                    PlayerLoop.SetPlayerLoop(modifiedLoop);
                else
                    Logger.LogError(nameof(SaveSystemCore), $"Failed remove system: {typeof(SaveSystemCore)}");
            }

        }

    }
#endif