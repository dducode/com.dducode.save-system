using SaveSystem.Internal.Cryptography;
using SaveSystem.Security;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    [CustomPropertyDrawer(typeof(AuthenticationSettings))]
    public class AuthenticationSettingsEditor : PropertyDrawer {

        private bool m_editProperties;


        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            var settings = (AuthenticationSettings)property.boxedValue;

            EditorGUILayout.LabelField("Authentication Settings", EditorStyles.boldLabel);

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
                "Hash Algorithm", settings.hashAlgorithm, GUILayout.MaxWidth(300)
            );
            settings.dataTablePassword = DrawingUtilities.DrawKeyProperty(
                settings.dataTablePassword, "Data Table Password", "Generate Password", CryptoUtilities.GenerateKey
            );

            GUI.enabled = true;
            EditorGUI.indentLevel--;

            property.boxedValue = settings;
        }

    }

}