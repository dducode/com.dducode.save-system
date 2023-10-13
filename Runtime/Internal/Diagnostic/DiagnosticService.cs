using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace SaveSystem.Internal.Diagnostic {

    internal static class DiagnosticService {

        internal static readonly List<HandlerMetadata> HandlersData = new();


        [Conditional("UNITY_EDITOR")]
        internal static void AddMetadata (HandlerMetadata metadata) {
            HandlersData.Add(metadata);
        }


        [Conditional("UNITY_EDITOR")]
        internal static void UpdateObjectsCount (int index, int count) {
            if (index >= HandlersData.Count)
                return;

            HandlerMetadata oldData = HandlersData[index];
            HandlersData[index] = new HandlerMetadata(
                oldData.destinationPath, oldData.caller, oldData.handlerType,
                oldData.objectsType, oldData.handle, count
            );
        }


    #if UNITY_EDITOR


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize () {
            var serviceLoop = new PlayerLoopSystem {
                type = typeof(DiagnosticService),
                updateDelegate = CheckHandlers
            };

            SetPlayerLoop(PlayerLoop.GetCurrentPlayerLoop(), serviceLoop);
            ResetOnExitPlayMode();
        }


        private static void ResetOnExitPlayMode () {
            EditorApplication.playModeStateChanged += state => {
                if (state is PlayModeStateChange.EnteredEditMode) {
                    HandlersData.Clear();
                    ResetPlayerLoop(PlayerLoop.GetCurrentPlayerLoop());
                }
            };
        }


        private static void CheckHandlers () {
            for (var i = 0; i < HandlersData.Count; i++) {
                if (HandlersData[i].handle.Target == null)
                    HandlersData.RemoveAt(i--);
            }
        }


        private static void SetPlayerLoop (PlayerLoopSystem modifiedLoop, PlayerLoopSystem serviceLoop) {
            if (PlayerLoopManager.TryInsertSubSystem(ref modifiedLoop, serviceLoop, typeof(PostLateUpdate)))
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            else
                Logger.LogError($"Failed insert system: {serviceLoop}");
        }


        private static void ResetPlayerLoop (PlayerLoopSystem modifiedLoop) {
            if (PlayerLoopManager.TryRemoveSubSystem(
                ref modifiedLoop, typeof(DiagnosticService), typeof(PostLateUpdate))
            ) {
                PlayerLoop.SetPlayerLoop(modifiedLoop);
            }
            else {
                Logger.LogError($"Failed remove system: {typeof(DiagnosticService)}");
            }
        }


    #endif

    }

}