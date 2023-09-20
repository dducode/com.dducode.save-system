#if UNITY_EDITOR
using System.Collections.Generic;
using SaveSystem.InternalServices.Diagnostic;
using UnityEditor;

namespace SaveSystem.Handlers {

    public static partial class ObjectHandlersFactory {

        internal static readonly List<HandlerMetadata> HandlersData = new();


        static ObjectHandlersFactory () {
            EditorApplication.playModeStateChanged += state => {
                if (state is PlayModeStateChange.ExitingPlayMode)
                    HandlersData.Clear();
            };
        }


        static partial void AddMetadata (HandlerMetadata metadata) {
            HandlersData.Add(metadata);
        }

    }

}
#endif