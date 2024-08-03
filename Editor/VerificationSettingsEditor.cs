using SaveSystemPackage.Internal.Cryptography;
using SaveSystemPackage.Security;
using SaveSystemPackage.Verification;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomPropertyDrawer(typeof(VerificationSettings))]
    public class VerificationSettingsEditor : PropertyDrawer {

        private bool m_editProperties;


        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            var settings = (VerificationSettings)property.boxedValue;

            EditorGUILayout.LabelField("Verification Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            m_editProperties = EditorGUILayout.ToggleLeft("Edit Properties", m_editProperties);
            GUI.enabled = m_editProperties;

            if (m_editProperties) {
                EditorGUILayout.HelpBox(
                    "It's unsafe action. It can make it impossible to verify the checksum of existing save files",
                    MessageType.Warning
                );
            }

            settings.hashAlgorithm = (HashAlgorithmName)EditorGUILayout.EnumPopup(
                "Hash Algorithm", settings.hashAlgorithm
            );
            settings.useCustomStorage = EditorGUILayout.Toggle("Use Custom Storage", settings.useCustomStorage);

            if (!settings.useCustomStorage) {
                settings.hashStoragePassword = DrawingUtilities.DrawKeyProperty(
                    settings.hashStoragePassword, "Hash Storage Password", "Generate Password",
                    CryptoUtilities.GenerateKey
                );
                if (string.IsNullOrEmpty(settings.hashStoragePath))
                    settings.hashStoragePath = "hash-storage";
                settings.hashStoragePath = EditorGUILayout.TextField("Hash Storage Name", settings.hashStoragePath);
            }

            GUI.enabled = true;
            EditorGUI.indentLevel--;

            property.boxedValue = settings;
        }

    }

}