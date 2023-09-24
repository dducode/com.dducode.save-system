#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace SaveSystem.InternalServices.Diagnostic {

    internal static partial class DiagnosticService {

        internal static readonly List<HandlerMetadata> HandlersData = new();


        static DiagnosticService () {
            EditorApplication.playModeStateChanged += state => {
                if (state is PlayModeStateChange.ExitingPlayMode)
                    HandlersData.Clear();
            };
        }


        internal static partial void AddMetadata (HandlerMetadata metadata) {
            HandlersData.Add(metadata);
        }


        internal static partial void AddObject (int index) {
            HandlerMetadata oldData = HandlersData[index];
            int objectsCount = oldData.objectsCount;
            objectsCount++;
            HandlersData[index] = new HandlerMetadata(
                oldData.filePath, oldData.caller, oldData.objectsType, objectsCount
            );
        }


        internal static partial void AddObjects (int index, int count) {
            HandlerMetadata oldData = HandlersData[index];
            int objectsCount = oldData.objectsCount + count;
            HandlersData[index] = new HandlerMetadata(
                oldData.filePath, oldData.caller, oldData.objectsType, objectsCount
            );
        }


        internal static partial void RemoveObject (int index) {
            HandlerMetadata oldData = HandlersData[index];
            int objectsCount = oldData.objectsCount;
            objectsCount--;
            HandlersData[index] = new HandlerMetadata(
                oldData.filePath, oldData.caller, oldData.objectsType, objectsCount
            );
        }

    }

}
#endif