using System.Linq;
using SaveSystemPackage.Internal;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomEditor(typeof(SceneSerializationScopeComponent), true)]
    public class SceneHandlerEditor : UnityEditor.Editor {

        private void OnEnable () {
            var sceneScope = (SceneSerializationScopeComponent)target;
            int sameObjects = sceneScope.gameObject.scene.GetRootGameObjects()
               .Count(go => go.TryGetComponent(out SceneSerializationScopeComponent _));
            if (sameObjects > 1)
                Debug.LogError("More than one scene loaders. It's not supported");

            if (!sceneScope.CompareTag(Tags.SceneScopeTag)) {
                PackageValidation.AddTagIfNotExists(Tags.SceneScopeTag);
                sceneScope.tag = Tags.SceneScopeTag;
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

}