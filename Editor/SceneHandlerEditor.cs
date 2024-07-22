using System.Linq;
using SaveSystemPackage.Internal;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomEditor(typeof(SceneHandler), true)]
    public class SceneHandlerEditor : UnityEditor.Editor {

        private void OnEnable () {
            var sceneLoader = (SceneHandler)target;
            int sameObjects = sceneLoader.gameObject.scene.GetRootGameObjects()
               .Count(go => go.TryGetComponent(out SceneHandler _));
            if (sameObjects > 1)
                Debug.LogError("More than one scene loaders. It's not supported");

            SerializedProperty sceneContext = serializedObject.FindProperty("sceneContext");
            if (sceneContext.boxedValue == null && sceneLoader.TryGetComponent(out SceneSerializationContext component))
                sceneContext.boxedValue = component;

            if (!sceneLoader.CompareTag(Tags.SceneHandlerTag)) {
                PackageValidation.AddTagIfNotExists(Tags.SceneHandlerTag);
                sceneLoader.tag = Tags.SceneHandlerTag;
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

}