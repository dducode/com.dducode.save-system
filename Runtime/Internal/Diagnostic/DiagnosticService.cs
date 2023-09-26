using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace SaveSystem.Internal.Diagnostic {

    internal static class DiagnosticService {

        internal static readonly List<HandlerMetadata> HandlersData = new();


    #if UNITY_EDITOR
        static DiagnosticService () {
            EditorApplication.playModeStateChanged += state => {
                if (state is PlayModeStateChange.ExitingPlayMode)
                    HandlersData.Clear();
            };
        }
    #endif


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
                oldData.destinationPath, oldData.caller, oldData.handlerType, oldData.objectsType, count
            );
        }

    }

}