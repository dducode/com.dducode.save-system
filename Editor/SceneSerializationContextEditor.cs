using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomEditor(typeof(SceneSerializationContext))]
    public class SceneSerializationContextEditor : UnityEditor.Editor {

        private SerializedProperty m_overrideSettingsProperty;
        private SerializedProperty m_encryptProperty;
        private SerializedProperty m_compressFilesProperty;


        private void OnEnable () {
            m_overrideSettingsProperty = serializedObject.FindProperty("overrideProjectSettings");
            m_encryptProperty = serializedObject.FindProperty("encrypt");
            m_compressFilesProperty = serializedObject.FindProperty("compressFiles");
        }


        public override void OnInspectorGUI () {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_overrideSettingsProperty);
            GUI.enabled = m_overrideSettingsProperty.boolValue;
            EditorGUILayout.PropertyField(m_encryptProperty);
            EditorGUILayout.PropertyField(m_compressFilesProperty);
            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }

    }

}