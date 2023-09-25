using SaveSystem.Core;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class SettingsTab : IConsoleTab {

        private readonly SerializedObject m_serializedSettings;

        private readonly SerializedProperty m_debugEnabledProperty;
        private readonly SerializedProperty m_autoSaveEnabledProperty;
        private readonly SerializedProperty m_savePeriodProperty;
        private readonly SerializedProperty m_saveModeProperty;

        private readonly SerializedProperty m_destroyCheckPointsProperty;
        private readonly SerializedProperty m_playerTagProperty;

        private readonly SerializedProperty m_registerImmediatelyProperty;

        private readonly GUIContent m_playerTagContent = new() {
            text = "Player Tag",
            tooltip = "Player tag is used to filtering messages from triggered checkpoints"
        };


        public SettingsTab () {
            m_serializedSettings = new SerializedObject(Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings)));

            m_autoSaveEnabledProperty = m_serializedSettings.FindProperty("autoSaveEnabled");
            m_savePeriodProperty = m_serializedSettings.FindProperty("savePeriod");
            m_saveModeProperty = m_serializedSettings.FindProperty("saveMode");
            m_debugEnabledProperty = m_serializedSettings.FindProperty("debugEnabled");

            m_destroyCheckPointsProperty = m_serializedSettings.FindProperty("destroyCheckPoints");
            m_playerTagProperty = m_serializedSettings.FindProperty("playerTag");

            m_registerImmediatelyProperty = m_serializedSettings.FindProperty("registerImmediately");
        }


        public void Draw () {
            m_serializedSettings.Update();

            DrawCoreSettings();

            EditorGUILayout.Space(15);
            DrawCheckpointsSettings();

            EditorGUILayout.Space(15);
            DrawFactorySettings();

            m_serializedSettings.ApplyModifiedProperties();
        }


        private void DrawCoreSettings () {
            EditorGUILayout.LabelField("Core Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_debugEnabledProperty);
            
            EditorGUILayout.PropertyField(m_autoSaveEnabledProperty);
            if (m_autoSaveEnabledProperty.boolValue)
                EditorGUILayout.PropertyField(m_savePeriodProperty, GUILayout.MaxWidth(300));
            
            EditorGUILayout.PropertyField(m_saveModeProperty, GUILayout.MaxWidth(300));
        }


        private void DrawCheckpointsSettings () {
            EditorGUILayout.LabelField("Checkpoints settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_destroyCheckPointsProperty);
            m_playerTagProperty.stringValue =
                EditorGUILayout.TagField(m_playerTagContent, m_playerTagProperty.stringValue, GUILayout.MaxWidth(300));
        }


        private void DrawFactorySettings () {
            EditorGUILayout.LabelField("Handlers Factory settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_registerImmediatelyProperty);
        }

    }

}