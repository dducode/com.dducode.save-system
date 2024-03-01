using System.Linq;
using SaveSystem.Internal;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    [CustomEditor(typeof(SceneLoader), true)]
    public class SceneLoaderEditor : UnityEditor.Editor {

        private void OnEnable () {
            var sceneLoader = (SceneLoader)target;
            int sameObjects = sceneLoader.gameObject.scene.GetRootGameObjects()
               .Count(go => go.TryGetComponent(out SceneLoader _));
            if (sameObjects > 1)
                Debug.LogError("More than one scene loaders. It's not supported");

            SerializedProperty sceneContext = serializedObject.FindProperty("sceneContext");
            if (sceneContext.boxedValue == null && sceneLoader.TryGetComponent(out SceneSerializationContext component))
                sceneContext.boxedValue = component;

            if (!sceneLoader.CompareTag(Tags.SceneLoaderTag)) {
                PackageValidation.AddTagIfNotExists(Tags.SceneLoaderTag);
                sceneLoader.tag = Tags.SceneLoaderTag;
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

}