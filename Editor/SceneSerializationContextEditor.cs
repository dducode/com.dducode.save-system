using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomEditor(typeof(SceneSerializationScope))]
    public class SceneSerializationContextEditor : UnityEditor.Editor {

        private SerializedProperty m_overrideSettingsProperty;
        private SerializedProperty m_compressFilesProperty;
        private SerializedProperty m_compressionSettingsProperty;
        private SerializedProperty m_encryptProperty;
        private SerializedProperty m_encryptionSettingsProperty;


        private void OnEnable () {
            m_overrideSettingsProperty = serializedObject.FindProperty("overrideProjectSettings");
            m_compressFilesProperty = serializedObject.FindProperty("compressFiles");
            m_compressionSettingsProperty = serializedObject.FindProperty("compressionSettings");
            m_encryptProperty = serializedObject.FindProperty("encrypt");
            m_encryptionSettingsProperty = serializedObject.FindProperty("encryptionSettings");
        }


        public override void OnInspectorGUI () {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_overrideSettingsProperty);

            GUI.enabled = m_overrideSettingsProperty.boolValue;
            EditorGUILayout.PropertyField(m_compressFilesProperty);

            if (m_overrideSettingsProperty.boolValue && m_compressFilesProperty.boolValue) {
                EditorGUILayout.PropertyField(m_compressionSettingsProperty);
                EditorGUILayout.Space(15);
            }

            EditorGUILayout.PropertyField(m_encryptProperty);
            if (m_overrideSettingsProperty.boolValue && m_encryptProperty.boolValue)
                EditorGUILayout.PropertyField(m_encryptionSettingsProperty);
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

    }

}