using SaveSystem.Internal.Cryptography;
using SaveSystem.Security;
using UnityEditor;

namespace SaveSystem.Editor {

    [CustomEditor(typeof(EncryptionSettings))]
    public class EncryptionSettingsEditor : UnityEditor.Editor {

        private SerializedProperty m_useCustomProvidersProperty;
        private SerializedProperty m_passwordProperty;
        private SerializedProperty m_saltKeyProperty;
        private SerializedProperty m_keyGenerationParamsProperty;


        private void OnEnable () {
            m_useCustomProvidersProperty = serializedObject.FindProperty("useCustomProviders");
            m_passwordProperty = serializedObject.FindProperty("password");
            m_saltKeyProperty = serializedObject.FindProperty("saltKey");
            m_keyGenerationParamsProperty = serializedObject.FindProperty("keyGenerationParams");
        }


        public override void OnInspectorGUI () {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_useCustomProvidersProperty);

            if (!m_useCustomProvidersProperty.boolValue) {
                DrawingUtilities.DrawKeyProperty(m_passwordProperty, "Generate Password", CryptoUtilities.GenerateKey);
                DrawingUtilities.DrawKeyProperty(m_saltKeyProperty, "Generate Salt Key", CryptoUtilities.GenerateKey);
            }

            EditorGUILayout.PropertyField(m_keyGenerationParamsProperty);

            serializedObject.ApplyModifiedProperties();
        }

    }

}