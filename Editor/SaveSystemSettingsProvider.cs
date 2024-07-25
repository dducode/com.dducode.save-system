#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaveSystemPackage.Editor {

    internal class SaveSystemSettingsProvider {

        public const string Path = "Project/Save System Settings";

        private readonly GUIContent m_playerTagContent = new() {
            text = "Player Tag",
            tooltip = "Tag is used to detect collisions from checkpoints"
        };

        private SerializedObject m_serializedSettings;

        private SerializedProperty m_automaticInitializeProperty;
        private SerializedProperty m_enabledSaveEventsProperty;
        private SerializedProperty m_enabledLogsProperty;

    #if ENABLE_BOTH_SYSTEMS
        private SerializedProperty m_usedInputSystemProperty;
    #endif

    #if ENABLE_LEGACY_INPUT_MANAGER
        private SerializedProperty m_quickSaveKeyProperty;
        private SerializedProperty m_screenCaptureKeyProperty;
    #endif

    #if ENABLE_INPUT_SYSTEM
        private SerializedProperty m_inputActionAssetProperty;
        private SerializedProperty m_quickSaveIdProperty;
        private SerializedProperty m_screenCaptureIdProperty;
    #endif

        private SerializedProperty m_savePeriodProperty;
        private SerializedProperty m_dataPathProperty;

        private SerializedProperty m_playerTagProperty;

        private SerializedProperty m_encryptProperty;
        private SerializedProperty m_encryptionSettingsProperty;

        private SerializedProperty m_authenticationProperty;
        private SerializedProperty m_authenticationSettingsProperty;


        [SettingsProvider]
        public static SettingsProvider CreateSaveSystemSettingsProvider () {
            return new SettingsProvider(Path, SettingsScope.Project) {
                label = "Save System",
                guiHandler = _ => {
                    var provider = new SaveSystemSettingsProvider();
                    provider.Draw();
                },
                keywords = new[] {"Save System"}
            };
        }


        private SaveSystemSettingsProvider () {
            Initialize(ResourcesManager.LoadSettings());
        }


        private void Draw () {
            if (m_serializedSettings == null || m_serializedSettings.targetObject == null) {
                if (ResourcesManager.TryLoadSettings(out SaveSystemSettings settings))
                    Initialize(settings);
                else
                    DrawFallbackWindow();

                return;
            }

            m_serializedSettings.Update();

            DrawCommonSettings();
            DrawUserActionsProperties();
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
                Initialize(EditorResourcesManager.CreateSettings());
        }


        private void Initialize (Object settings) {
            if (settings == null)
                return;

            m_serializedSettings = new SerializedObject(settings);

            InitializeCommonSettings();
            InitializeUserActionsProperties();
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


        private void InitializeUserActionsProperties () {
        #if ENABLE_BOTH_SYSTEMS
            m_usedInputSystemProperty = m_serializedSettings.FindProperty("usedInputSystem");
        #endif

        #if ENABLE_LEGACY_INPUT_MANAGER
            m_quickSaveKeyProperty = m_serializedSettings.FindProperty("quickSaveKey");
            m_screenCaptureKeyProperty = m_serializedSettings.FindProperty("screenCaptureKey");
        #endif

        #if ENABLE_INPUT_SYSTEM
            m_inputActionAssetProperty = m_serializedSettings.FindProperty("inputActionAsset");
            m_quickSaveIdProperty = m_serializedSettings.FindProperty("quickSaveId");
            m_screenCaptureIdProperty = m_serializedSettings.FindProperty("screenCaptureId");
        #endif
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
            GUI.enabled = saveEvents.HasFlag(SaveEvents.PeriodicSave);
            EditorGUILayout.PropertyField(m_savePeriodProperty, GUILayout.MaxWidth(300));
            GUI.enabled = true;

            EditorGUILayout.PropertyField(m_dataPathProperty, GUILayout.MaxWidth(500));
            if (string.IsNullOrEmpty(m_dataPathProperty.stringValue))
                m_dataPathProperty.stringValue = $"{Application.productName.ToPathFormat()}.data";

            EditorGUILayout.Space(15);
        }


        private void DrawUserActionsProperties () {
            EditorGUILayout.LabelField("User Actions", EditorStyles.boldLabel);

        #if ENABLE_BOTH_SYSTEMS
            EditorGUILayout.PropertyField(m_usedInputSystemProperty, GUILayout.MaxWidth(300));

            switch ((UsedInputSystem)m_usedInputSystemProperty.enumValueIndex) {
                case UsedInputSystem.LegacyInputManager:
                    EditorGUILayout.PropertyField(m_quickSaveKeyProperty, GUILayout.MaxWidth(300));
                    EditorGUILayout.PropertyField(m_screenCaptureKeyProperty, GUILayout.MaxWidth(300));
                    break;
                case UsedInputSystem.InputSystem:
                    EditorGUILayout.PropertyField(m_inputActionAssetProperty, GUILayout.MaxWidth(300));
                    EditorGUILayout.PropertyField(m_quickSaveIdProperty, GUILayout.MaxWidth(300));
                    EditorGUILayout.PropertyField(m_screenCaptureIdProperty, GUILayout.MaxWidth(300));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        #else
        #if ENABLE_LEGACY_INPUT_MANAGER
            EditorGUILayout.PropertyField(m_quickSaveKeyProperty, GUILayout.MaxWidth(300));
            EditorGUILayout.PropertyField(m_screenCaptureKeyProperty, GUILayout.MaxWidth(300));
        #endif

        #if ENABLE_INPUT_SYSTEM
            EditorGUILayout.PropertyField(m_inputActionAssetProperty, GUILayout.MaxWidth(300));
            EditorGUILayout.PropertyField(m_quickSaveActionNameProperty, GUILayout.MaxWidth(300));
            EditorGUILayout.PropertyField(m_screenCaptureActionNameProperty, GUILayout.MaxWidth(300));
        #endif
        #endif

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
            EditorGUILayout.PropertyField(m_encryptProperty);

            if (m_encryptProperty.boolValue) {
                EditorGUILayout.PropertyField(m_encryptionSettingsProperty, GUILayout.MaxWidth(500));
                EditorGUILayout.Space(15);
            }
        }


        private void DrawAuthenticationSettings () {
            EditorGUILayout.PropertyField(m_authenticationProperty);

            if (m_authenticationProperty.boolValue) {
                EditorGUILayout.PropertyField(m_authenticationSettingsProperty, GUILayout.MaxWidth(500));
                EditorGUILayout.Space(15);
            }
        }

    }

}