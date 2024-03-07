using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class SettingsTab : IConsoleTab {

        private readonly GUIContent m_playerTagContent = new() {
            text = "Player Tag",
            tooltip = "Tag is used to detect collisions from checkpoints"
        };

        private SerializedObject m_serializedSettings;

        private SerializedProperty m_enabledSaveEventsProperty;
        private SerializedProperty m_enabledLogsProperty;
        private SerializedProperty m_savePeriodProperty;
        private SerializedProperty m_isParallelProperty;
        private SerializedProperty m_dataPathProperty;

        private SerializedProperty m_playerTagProperty;

        private SerializedProperty m_encryptProperty;
        private SerializedProperty m_encryptionSettingsProperty;


        public SettingsTab () {
            Initialize(Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings)));
        }


        public void Draw () {
            if (m_serializedSettings == null || m_serializedSettings.targetObject == null) {
                if (TryLoadSettings(out SaveSystemSettings settings))
                    Initialize(settings);
                else
                    DrawFallbackWindow();

                return;
            }

            m_serializedSettings.Update();

            DrawCoreSettings();
            DrawCheckpointsSettings();
            DrawEncryptionSettings();

            m_serializedSettings.ApplyModifiedProperties();
        }


        private bool TryLoadSettings (out SaveSystemSettings settings) {
            settings = Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings));
            return settings != null;
        }


        private void DrawFallbackWindow () {
            EditorGUILayout.HelpBox(
                "You are missing settings asset. Click button to restore it", MessageType.Warning
            );

            if (GUILayout.Button("Restore Settings Asset", GUILayout.ExpandWidth(false)))
                Initialize(SaveSystemTools.CreateSettings());
        }


        private void Initialize (Object settings) {
            if (settings == null)
                return;

            m_serializedSettings = new SerializedObject(settings);

            m_enabledSaveEventsProperty = m_serializedSettings.FindProperty("enabledSaveEvents");
            m_enabledLogsProperty = m_serializedSettings.FindProperty("enabledLogs");

            m_savePeriodProperty = m_serializedSettings.FindProperty("savePeriod");
            m_isParallelProperty = m_serializedSettings.FindProperty("isParallel");
            m_dataPathProperty = m_serializedSettings.FindProperty("dataPath");

            m_playerTagProperty = m_serializedSettings.FindProperty("playerTag");

            m_encryptProperty = m_serializedSettings.FindProperty("encryption");
            m_encryptionSettingsProperty = m_serializedSettings.FindProperty("encryptionSettings");

            m_serializedSettings.FindProperty("registerImmediately");
        }


        private void DrawCoreSettings () {
            EditorGUILayout.LabelField("Common Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_enabledSaveEventsProperty, GUILayout.MaxWidth(300));
            EditorGUILayout.PropertyField(m_enabledLogsProperty, GUILayout.MaxWidth(300));

            var saveEvents = (SaveEvents)m_enabledSaveEventsProperty.enumValueFlag;
            if (saveEvents.HasFlag(SaveEvents.AutoSave))
                EditorGUILayout.PropertyField(m_savePeriodProperty, GUILayout.MaxWidth(300));

            EditorGUILayout.PropertyField(m_isParallelProperty);
            EditorGUILayout.PropertyField(m_dataPathProperty, GUILayout.MaxWidth(500));

            EditorGUILayout.Space(15);
        }


        private void DrawCheckpointsSettings () {
            EditorGUILayout.LabelField("Checkpoints settings", EditorStyles.boldLabel);
            m_playerTagProperty.stringValue = EditorGUILayout.TagField(
                m_playerTagContent, m_playerTagProperty.stringValue, GUILayout.MaxWidth(300)
            );

            EditorGUILayout.Space(15);
        }


        private void DrawEncryptionSettings () {
            EditorGUILayout.LabelField("Encryption settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_encryptProperty);

            if (m_encryptProperty.boolValue) 
                EditorGUILayout.PropertyField(m_encryptionSettingsProperty, GUILayout.MaxWidth(500));

            EditorGUILayout.Space(15);
        }

    }

}