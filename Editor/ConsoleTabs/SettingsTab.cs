using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class SettingsTab : IConsoleTab {

        private SerializedObject m_serializedSettings;

        private SerializedProperty m_enabledSaveEventsProperty;
        private SerializedProperty m_savePeriodProperty;
        private SerializedProperty m_isParallelProperty;
        private SerializedProperty m_enabledLogsProperty;

        private SerializedProperty m_destroyCheckPointsProperty;
        private SerializedProperty m_playerTagProperty;

        private SerializedProperty m_registerImmediatelyProperty;

        private readonly GUIContent m_saveEventsContent = new() {
            text = "Enabled Save Events",
            tooltip =
                "It's used to manage autosave loop, save on focus changed, on low memory and on quitting the game\n\n" +
                "<b>Note:</b> save on quitting is not supported in the editor"
        };

        private readonly GUIContent m_enabledLogsContent = new() {
            text = "Enabled Logs",
            tooltip = "Choose which logs should be enabled - none, debug, warning, error or all"
        };

        private readonly GUIContent m_playerTagContent = new() {
            text = "Player Tag",
            tooltip = "Player tag is used to filtering messages from triggered checkpoints"
        };

        private SaveEvents m_enabledSaveEvents;
        private LogLevel m_enabledLogs;


        public SettingsTab () {
            var settings = Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings));

            if (settings == null)
                return;

            Initialize(settings);
        }


        public void Draw () {
            if (m_serializedSettings == null) {
                EditorGUILayout.HelpBox(
                    "You are missing settings asset. Click button to restore it", MessageType.Warning
                );

                if (GUILayout.Button("Restore Settings Asset", GUILayout.ExpandWidth(false))) {
                    SaveSystemSettings.CreateSettings();
                    Initialize(Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings)));
                }

                return;
            }

            m_serializedSettings.Update();

            DrawCoreSettings();

            EditorGUILayout.Space(15);
            DrawCheckpointsSettings();

            EditorGUILayout.Space(15);
            DrawFactorySettings();

            m_serializedSettings.ApplyModifiedProperties();
        }


        private void Initialize (Object settings) {
            m_serializedSettings = new SerializedObject(settings);

            m_enabledSaveEventsProperty = m_serializedSettings.FindProperty("enabledSaveEvents");
            m_enabledSaveEvents = (SaveEvents)m_enabledSaveEventsProperty.enumValueFlag;
            m_enabledLogsProperty = m_serializedSettings.FindProperty("enabledLogs");
            m_enabledLogs = (LogLevel)m_enabledLogsProperty.enumValueFlag;

            m_savePeriodProperty = m_serializedSettings.FindProperty("savePeriod");
            m_isParallelProperty = m_serializedSettings.FindProperty("isParallel");

            m_destroyCheckPointsProperty = m_serializedSettings.FindProperty("destroyCheckPoints");
            m_playerTagProperty = m_serializedSettings.FindProperty("playerTag");

            m_registerImmediatelyProperty = m_serializedSettings.FindProperty("registerImmediately");
        }


        private void DrawCoreSettings () {
            EditorGUILayout.LabelField("Core Settings", EditorStyles.boldLabel);

            m_enabledSaveEvents = (SaveEvents)EditorGUILayout.EnumFlagsField(
                m_saveEventsContent,
                m_enabledSaveEvents,
                GUILayout.MaxWidth(300)
            );

            m_enabledSaveEventsProperty.enumValueFlag = (int)m_enabledSaveEvents;

            if (m_enabledSaveEvents.HasFlag(SaveEvents.AutoSave))
                EditorGUILayout.PropertyField(m_savePeriodProperty, GUILayout.MaxWidth(300));

            EditorGUILayout.PropertyField(m_isParallelProperty);

            m_enabledLogs = (LogLevel)EditorGUILayout.EnumFlagsField(
                m_enabledLogsContent,
                m_enabledLogs,
                GUILayout.MaxWidth(300)
            );

            m_enabledLogsProperty.enumValueFlag = (int)m_enabledLogs;
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