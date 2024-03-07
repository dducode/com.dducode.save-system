using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    [CustomEditor(typeof(SceneSerializationContext))]
    public class SceneSerializationContextEditor : UnityEditor.Editor {

        private SerializedProperty m_encryptProperty;
        private SerializedProperty m_encryptionSettingsProperty;


        private void OnEnable () {
            var sceneContext = (SceneSerializationContext)target;
            int sameObjects = sceneContext.gameObject.scene.GetRootGameObjects()
               .Count(go => go.TryGetComponent(out SceneSerializationContext _));
            if (sameObjects > 1)
                Debug.LogError("More than one scene serialization contexts. It's not supported");

            m_encryptProperty = serializedObject.FindProperty("encrypt");
            m_encryptionSettingsProperty = serializedObject.FindProperty("encryptionSettings");
        }


        public override void OnInspectorGUI () {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_encryptProperty);

            if (m_encryptProperty.boolValue)
                EditorGUILayout.PropertyField(m_encryptionSettingsProperty);

            serializedObject.ApplyModifiedProperties();
        }

    }

}