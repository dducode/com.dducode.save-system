using SaveSystem.Internal;
using SaveSystem.Security;
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

        private SerializedProperty m_automaticInitializeProperty;
        private SerializedProperty m_enabledSaveEventsProperty;
        private SerializedProperty m_enabledLogsProperty;
        private SerializedProperty m_savePeriodProperty;
        private SerializedProperty m_dataPathProperty;

        private SerializedProperty m_playerTagProperty;

        private SerializedProperty m_encryptProperty;
        private SerializedProperty m_encryptionSettingsProperty;

        private SerializedProperty m_authenticationProperty;
        private SerializedProperty m_authenticationSettingsProperty;


        public SettingsTab () {
            Initialize(ResourcesManager.LoadSettings<SaveSystemSettings>());
        }


        public void Draw () {
            if (m_serializedSettings == null || m_serializedSettings.targetObject == null) {
                if (ResourcesManager.TryLoadSettings(out SaveSystemSettings settings))
                    Initialize(settings);
                else
                    DrawFallbackWindow();

                return;
            }

            m_serializedSettings.Update();

            DrawCommonSettings();
            DrawCheckpointsSettings();
            DrawEncryptionSettings();
            DrawAuthenticationSettings();

            m_serializedSettings.ApplyModifiedProperties();
        }


        private void DrawFallbackWindow () {
            EditorGUILayout.HelpBox(
                "You are missing settings asset. Click button to restore it", MessageType.Warning
            );

            if (GUILayout.Button("Restore Settings Asset", GUILayout.ExpandWidth(false)))
                Initialize(EditorResourcesManager.CreateSettings<SaveSystemSettings>());
        }


        private void Initialize (Object settings) {
            if (settings == null)
                return;

            m_serializedSettings = new SerializedObject(settings);

            InitializeCommonSettings();
            InitializeCheckpointsSettings();
            InitializeEncryptionSettings();
            InitializeAuthSettings();

            m_serializedSettings.FindProperty("registerImmediately");
        }


        private void InitializeCommonSettings () {
            m_automaticInitializeProperty = m_serializedSettings.FindProperty("automaticInitialize");
            m_enabledSaveEventsProperty = m_serializedSettings.FindProperty("enabledSaveEvents");
            m_enabledLogsProperty = m_serializedSettings.FindProperty("enabledLogs");
            m_savePeriodProperty = m_serializedSettings.FindProperty("savePeriod");
            m_dataPathProperty = m_serializedSettings.FindProperty("dataPath");
        }


        private void InitializeCheckpointsSettings () {
            m_playerTagProperty = m_serializedSettings.FindProperty("playerTag");
        }


        private void InitializeEncryptionSettings () {
            m_encryptProperty = m_serializedSettings.FindProperty("encryption");
            m_encryptionSettingsProperty = m_serializedSettings.FindProperty("encryptionSettings");
        }


        private void InitializeAuthSettings () {
            m_authenticationProperty = m_serializedSettings.FindProperty("authentication");
            m_authenticationSettingsProperty = m_serializedSettings.FindProperty("authenticationSettings");
        }


        private void DrawCommonSettings () {
            EditorGUILayout.LabelField("Common Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_automaticInitializeProperty, GUILayout.MaxWidth(300));
            EditorGUILayout.PropertyField(m_enabledSaveEventsProperty, GUILayout.MaxWidth(300));
            EditorGUILayout.PropertyField(m_enabledLogsProperty, GUILayout.MaxWidth(300));

            var saveEvents = (SaveEvents)m_enabledSaveEventsProperty.enumValueFlag;
            if (saveEvents.HasFlag(SaveEvents.AutoSave))
                EditorGUILayout.PropertyField(m_savePeriodProperty, GUILayout.MaxWidth(300));

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

            if (m_encryptProperty.boolValue) {
                m_encryptionSettingsProperty.boxedValue ??=
                    EditorResourcesManager.CreateSettings<EncryptionSettings>();
                EditorGUILayout.PropertyField(m_encryptionSettingsProperty, GUILayout.MaxWidth(500));
            }

            EditorGUILayout.Space(15);
        }


        private void DrawAuthenticationSettings () {
            EditorGUILayout.LabelField("Authentication settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_authenticationProperty);

            if (m_authenticationProperty.boolValue) {
                m_authenticationSettingsProperty.boxedValue ??=
                    EditorResourcesManager.CreateSettings<AuthenticationSettings>();
                EditorGUILayout.PropertyField(m_authenticationSettingsProperty, GUILayout.MaxWidth(500));
            }

            EditorGUILayout.Space(15);
        }

    }

}