#if UNITY_EDITOR
    using SaveSystem.Internal;
    using SaveSystem.Internal.Diagnostic;
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
                m_handler = null;
                DiagnosticService.Clear();

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