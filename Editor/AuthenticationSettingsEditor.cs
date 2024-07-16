using System;
using SaveSystem.Security;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    [CustomPropertyDrawer(typeof(AuthenticationSettings))]
    public class AuthenticationSettingsEditor : PropertyDrawer {

        private bool m_foldout;
        private bool m_editProperties;


        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            var settings = (AuthenticationSettings)property.boxedValue;

            m_foldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldout, "Authentication Settings");

            if (m_foldout) {
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
                settings.globalAuthHashKey = DrawingUtilities.DrawKeyProperty(
                    settings.globalAuthHashKey, "Global Auth Hash Key", "Generate Global Key", Guid.NewGuid().ToString
                );
                settings.profileAuthHashKey = DrawingUtilities.DrawKeyProperty(
                    settings.profileAuthHashKey, "Profile Auth Hash Key",
                    "Generate Profile Key", Guid.NewGuid().ToString
                );

                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            property.boxedValue = settings;
        }

    }

}