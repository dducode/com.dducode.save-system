using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    [CustomEditor(typeof(SceneSerializationContext))]
    public class SceneSerializationContextEditor : UnityEditor.Editor {

        private void OnEnable () {
            var sceneContext = (SceneSerializationContext)target;
            int sameObjects = sceneContext.gameObject.scene.GetRootGameObjects()
               .Count(go => go.TryGetComponent(out SceneSerializationContext _));
            if (sameObjects > 1)
                Debug.LogError("More than one scene serialization contexts. It's not supported");

            SerializedProperty sceneName = serializedObject.FindProperty("sceneName");
            if (string.IsNullOrEmpty(sceneName.stringValue))
                sceneName.stringValue = sceneContext.gameObject.scene.name;

            serializedObject.ApplyModifiedProperties();
        }

    }

}