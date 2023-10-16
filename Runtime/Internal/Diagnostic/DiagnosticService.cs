using System.Collections.Generic;
using System.Diagnostics;

namespace SaveSystem.Internal.Diagnostic {

    internal static class DiagnosticService {

        internal static readonly List<HandlerMetadata> HandlersData = new();
        internal static int HandlersCount => HandlersData.Count;


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


        [Conditional("UNITY_EDITOR")]
        internal static void CheckHandlers () {
            for (var i = 0; i < HandlersData.Count; i++)
                if (HandlersData[i].handle.Target == null)
                    HandlersData.RemoveAt(i--);
        }

    }

}