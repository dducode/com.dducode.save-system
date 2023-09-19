#if UNITY_EDITOR
using System.Collections.Generic;
using SaveSystem.InternalServices.Diagnostic;
using UnityEditor;

namespace SaveSystem.Handlers {

    public static partial class HandlersProvider {

        internal static readonly List<HandlerMetadata> HandlersData = new();


        static HandlersProvider () {
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