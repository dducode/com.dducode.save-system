using SaveSystem.Core;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class SettingsTab : IConsoleTab {

        private readonly SerializedObject m_serializedSettings;

        private readonly SerializedProperty m_autoSaveEnabledProperty;
        private readonly SerializedProperty m_savePeriodProperty;
        private readonly SerializedProperty m_asyncSaveEnabledProperty;
        private readonly SerializedProperty m_debugEnabledProperty;

        private readonly SerializedProperty m_destroyCheckPointsProperty;
        private readonly SerializedProperty m_playerTagProperty;

        private readonly SerializedProperty m_registerImmediatelyProperty;

        private readonly GUIContent m_savePeriodContent = new() {
            text = "Save Period",
            tooltip = "For what period should it be saved automatically? (in sec.)"
        };

        private readonly GUIContent m_destroyCheckPointsContent = new() {
            text = "Destroy Check Points",
            tooltip = "Destroy check points after saving?"
        };


        public SettingsTab () {
            m_serializedSettings = new SerializedObject(Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings)));

            m_autoSaveEnabledProperty = m_serializedSettings.FindProperty("autoSaveEnabled");
            m_savePeriodProperty = m_serializedSettings.FindProperty("savePeriod");
            m_asyncSaveEnabledProperty = m_serializedSettings.FindProperty("asyncSaveEnabled");
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
            DrawProviderSettings();

            m_serializedSettings.ApplyModifiedProperties();
        }


        private void DrawCoreSettings () {
            EditorGUILayout.LabelField("Core Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_autoSaveEnabledProperty);
            if (m_autoSaveEnabledProperty.boolValue)
                EditorGUILayout.PropertyField(m_savePeriodProperty, m_savePeriodContent, GUILayout.MaxWidth(300));
            EditorGUILayout.PropertyField(m_asyncSaveEnabledProperty);
            EditorGUILayout.PropertyField(m_debugEnabledProperty);
        }


        private void DrawCheckpointsSettings () {
            EditorGUILayout.LabelField("Checkpoints settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_destroyCheckPointsProperty, m_destroyCheckPointsContent);
            m_playerTagProperty.stringValue =
                EditorGUILayout.TagField("Player Tag", m_playerTagProperty.stringValue, GUILayout.MaxWidth(300));
        }


        private void DrawProviderSettings () {
            EditorGUILayout.LabelField("Handlers Provider settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_registerImmediatelyProperty);
        }

    }

}