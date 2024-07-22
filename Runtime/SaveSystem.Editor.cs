#if UNITY_EDITOR
    using SaveSystemPackage.Internal;
    using SaveSystemPackage.Internal.Diagnostic;
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
                OnSaveStart = null;
                OnSaveEnd = null;

                Application.wantsToQuit -= SaveBeforeExit;
                Application.focusChanged -= OnFocusLost;
                Application.lowMemory -= OnLowMemory;

            #if ENABLE_LEGACY_INPUT_MANAGER
                m_quickSaveKey = default;
            #endif

            #if ENABLE_INPUT_SYSTEM
                m_quickSaveAction = null;
            #endif

                m_autoSaveLastTime = 0;
                m_savedBeforeExit = false;

                if (m_selectedSaveProfile != null) {
                    m_selectedSaveProfile.Clear();
                    m_selectedSaveProfile = null;
                }

                m_globalScope.Clear();
                m_globalScope = null;
                DiagnosticService.Clear();

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