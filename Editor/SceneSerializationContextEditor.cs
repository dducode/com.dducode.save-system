using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    [CustomEditor(typeof(SceneSerializationContext))]
    public class SceneSerializationContextEditor : UnityEditor.Editor {

        private SerializedProperty m_encryptProperty;
        private SerializedProperty m_encryptionSettingsProperty;

        private SerializedProperty m_authenticationProperty;
        private SerializedProperty m_algorithmNameProperty;


        private void OnEnable () {
            var sceneContext = (SceneSerializationContext)target;
            int sameObjects = sceneContext.gameObject.scene.GetRootGameObjects()
               .Count(go => go.TryGetComponent(out SceneSerializationContext _));
            if (sameObjects > 1)
                Debug.LogError("More than one scene serialization contexts. It's not supported");

            InitializeEncryptionProperties();
            InitializeAuthProperties();
        }


        public override void OnInspectorGUI () {
            serializedObject.Update();
            DrawEncryptionProperties();
            DrawAuthProperties();
            serializedObject.ApplyModifiedProperties();
        }


        private void InitializeEncryptionProperties () {
            m_encryptProperty = serializedObject.FindProperty("encrypt");
            m_encryptionSettingsProperty = serializedObject.FindProperty("encryptionSettings");
        }


        private void InitializeAuthProperties () {
            m_authenticationProperty = serializedObject.FindProperty("authentication");
            m_algorithmNameProperty = serializedObject.FindProperty("algorithmName");
        }


        private void DrawEncryptionProperties () {
            EditorGUILayout.PropertyField(m_encryptProperty);
            if (m_encryptProperty.boolValue)
                EditorGUILayout.PropertyField(m_encryptionSettingsProperty);
        }


        private void DrawAuthProperties () {
            EditorGUILayout.PropertyField(m_authenticationProperty);
            if (m_authenticationProperty.boolValue)
                EditorGUILayout.PropertyField(m_algorithmNameProperty);
        }

    }

}