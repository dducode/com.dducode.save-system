using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace SaveSystem.InternalServices.Diagnostic {

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
            // bug argument out of range
            if (index >= HandlersData.Count)
                return;

            HandlerMetadata oldData = HandlersData[index];
            HandlersData[index] = new HandlerMetadata(
                oldData.filePath, oldData.caller, oldData.objectsType, count
            );
        }

    }

}