using SaveSystemPackage.Security;
using SaveSystemPackage.Settings;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomPropertyDrawer(typeof(EncryptionSettings))]
    public class EncryptionSettingsPropertyDrawer : PropertyDrawer {

        private bool m_editProperties;
        private bool m_foldout = true;
        private bool m_showSecureValues;


        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            var settings = (EncryptionSettings)property.boxedValue;
            EditorGUILayout.LabelField("Encryption Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            m_editProperties = EditorGUILayout.ToggleLeft("Edit Properties", m_editProperties);
            if (!settings.useCustomCryptographer && !settings.useCustomProviders)
                m_showSecureValues = EditorGUILayout.ToggleLeft("Show Secure Values", m_showSecureValues);
            GUI.enabled = m_editProperties;

            if (m_editProperties) {
                EditorGUILayout.HelpBox(
                    "It's unsafe action. It can make it impossible to decrypt existing save files",
                    MessageType.Warning
                );
            }

            settings.useCustomCryptographer = EditorGUILayout.Toggle(
                "Use Custom Cryptographer", settings.useCustomCryptographer
            );

            if (settings.useCustomCryptographer) {
                EditorGUILayout.ObjectField(
                    "Cryptographer", settings.reference, typeof(CryptographerReference), false
                );
            }
            else {
                settings.useCustomProviders = EditorGUILayout.Toggle(
                    "Use Custom Providers", settings.useCustomProviders
                );

                if (!settings.useCustomProviders) {
                    settings.password = DrawingUtilities.DrawKeyProperty(
                        settings.password, "Password", "Generate Password", m_showSecureValues
                    );
                    settings.saltKey = DrawingUtilities.DrawKeyProperty(
                        settings.saltKey, "Salt Key", "Generate Salt Key", m_showSecureValues
                    );
                }

                m_foldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldout, "Key Generation Parameters");

                if (m_foldout) {
                    EditorGUI.indentLevel++;
                    KeyGenerationParams keyParams = settings.keyGenerationParams;
                    keyParams.keyLength = (AESKeyLength)EditorGUILayout.EnumPopup(
                        "Key Length", keyParams.keyLength
                    );
                    keyParams.hashAlgorithm = (HashAlgorithmName)EditorGUILayout.EnumPopup(
                        "Hash Algorithm", keyParams.hashAlgorithm
                    );
                    keyParams.iterations = EditorGUILayout.IntField("Iterations", keyParams.iterations);
                    settings.keyGenerationParams = keyParams;
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            GUI.enabled = true;
            EditorGUI.indentLevel--;

            property.boxedValue = settings;
        }

    }

}