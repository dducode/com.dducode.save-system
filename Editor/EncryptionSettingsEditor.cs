using SaveSystemPackage.Internal.Cryptography;
using SaveSystemPackage.Security;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    [CustomPropertyDrawer(typeof(EncryptionSettings))]
    public class EncryptionSettingsEditor : PropertyDrawer {

        private bool m_editProperties;


        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            var settings = (EncryptionSettings)property.boxedValue;

            EditorGUILayout.LabelField("Encryption Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            m_editProperties = EditorGUILayout.ToggleLeft("Edit Properties", m_editProperties);
            GUI.enabled = m_editProperties;

            if (m_editProperties) {
                EditorGUILayout.HelpBox(
                    "It's unsafe action. It can make it impossible to decrypt existing save files",
                    MessageType.Warning
                );
            }

            settings.useCustomProviders = EditorGUILayout.Toggle(
                "Use Custom Providers", settings.useCustomProviders
            );

            if (!settings.useCustomProviders) {
                settings.password = DrawingUtilities.DrawKeyProperty(
                    settings.password, "Password", "Generate Password", CryptoUtilities.GenerateKey
                );
                settings.saltKey = DrawingUtilities.DrawKeyProperty(
                    settings.saltKey, "Salt Key", "Generate Salt Key", CryptoUtilities.GenerateKey
                );
            }

            EditorGUILayout.LabelField("Key Generation Parameters");
            KeyGenerationParams keyParams = settings.keyGenerationParams;
            keyParams.keyLength = (AESKeyLength)EditorGUILayout.EnumPopup(
                "Key Length", keyParams.keyLength, GUILayout.MaxWidth(300)
            );
            keyParams.hashAlgorithm = (HashAlgorithmName)EditorGUILayout.EnumPopup(
                "Hash Algorithm", keyParams.hashAlgorithm, GUILayout.MaxWidth(300)
            );
            keyParams.iterations = EditorGUILayout.IntField(
                "Iterations", keyParams.iterations, GUILayout.MaxWidth(500)
            );
            settings.keyGenerationParams = keyParams;

            GUI.enabled = true;
            EditorGUI.indentLevel--;

            EditorGUILayout.EndFoldoutHeaderGroup();

            property.boxedValue = settings;
        }

    }

}