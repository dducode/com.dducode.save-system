#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
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
        private SerializedProperty m_quickSaveActionProperty;
        private SerializedProperty m_screenCaptureActionProperty;
    #endif

        private SerializedProperty m_savePeriodProperty;
        private SerializedProperty m_dataFileNameProperty;

        private SerializedProperty m_playerTagProperty;

        private SerializedProperty m_encryptProperty;
        private SerializedProperty m_encryptionSettingsProperty;


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
            Initialize(SaveSystemSettings.Load());
        }


        private void Draw () {
            if (m_serializedSettings == null || m_serializedSettings.targetObject == null) {
                if (SaveSystemSettings.TryLoad(out SaveSystemSettings settings))
                    Initialize(settings);
                else
                    DrawFallbackWindow();

                return;
            }

            m_serializedSettings.Update();

            float width = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth *= 1.5f;
            DrawCommonSettings();
            DrawUserActionsProperties();
            DrawCheckpointsSettings();
            DrawEncryptionSettings();
            EditorGUIUtility.labelWidth = width;

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

            m_serializedSettings.FindProperty("registerImmediately");
        }


        private void InitializeCommonSettings () {
            m_automaticInitializeProperty = m_serializedSettings.FindProperty(
                nameof(SaveSystemSettings.automaticInitialize)
            );
            m_enabledSaveEventsProperty = m_serializedSettings.FindProperty(
                nameof(SaveSystemSettings.enabledSaveEvents)
            );
            m_enabledLogsProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.enabledLogs));
            m_savePeriodProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.savePeriod));
            m_dataFileNameProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.dataFileName));
        }


        private void InitializeUserActionsProperties () {
        #if ENABLE_BOTH_SYSTEMS
            m_usedInputSystemProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.usedInputSystem));
        #endif

        #if ENABLE_LEGACY_INPUT_MANAGER
            m_quickSaveKeyProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.quickSaveKey));
            m_screenCaptureKeyProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.screenCaptureKey));
        #endif

        #if ENABLE_INPUT_SYSTEM
            m_quickSaveActionProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.quickSaveAction));
            m_screenCaptureActionProperty = m_serializedSettings.FindProperty(
                nameof(SaveSystemSettings.screenCaptureAction)
            );
        #endif
        }


        private void InitializeCheckpointsSettings () {
            m_playerTagProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.playerTag));
        }


        private void InitializeEncryptionSettings () {
            m_encryptProperty = m_serializedSettings.FindProperty(nameof(SaveSystemSettings.encrypt));
            m_encryptionSettingsProperty = m_serializedSettings.FindProperty(
                nameof(SaveSystemSettings.encryptionSettings)
            );
        }


        private void DrawCommonSettings () {
            EditorGUILayout.LabelField("Common Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_automaticInitializeProperty);
            EditorGUILayout.PropertyField(m_enabledLogsProperty);
            EditorGUILayout.PropertyField(m_enabledSaveEventsProperty);

            var saveEvents = (SaveEvents)m_enabledSaveEventsProperty.enumValueFlag;
            GUI.enabled = saveEvents.HasFlag(SaveEvents.PeriodicSave);
            EditorGUILayout.PropertyField(m_savePeriodProperty);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(m_dataFileNameProperty);
            if (string.IsNullOrEmpty(m_dataFileNameProperty.stringValue))
                m_dataFileNameProperty.stringValue = Application.productName.ToPathFormat();

            EditorGUILayout.Space(15);
        }


        private void DrawUserActionsProperties () {
            EditorGUILayout.LabelField("User Actions", EditorStyles.boldLabel);

        #if ENABLE_BOTH_SYSTEMS
            EditorGUILayout.PropertyField(m_usedInputSystemProperty);

            switch ((UsedInputSystem)m_usedInputSystemProperty.enumValueIndex) {
                case UsedInputSystem.LegacyInputManager:
                    EditorGUILayout.PropertyField(m_quickSaveKeyProperty);
                    EditorGUILayout.PropertyField(m_screenCaptureKeyProperty);
                    break;
                case UsedInputSystem.InputSystem:
                    EditorGUILayout.PropertyField(m_quickSaveActionProperty);
                    EditorGUILayout.PropertyField(m_screenCaptureActionProperty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        #else
        #if ENABLE_LEGACY_INPUT_MANAGER
            EditorGUILayout.PropertyField(m_quickSaveKeyProperty);
            EditorGUILayout.PropertyField(m_screenCaptureKeyProperty);
        #endif

        #if ENABLE_INPUT_SYSTEM
            EditorGUILayout.PropertyField(m_quickSaveActionNameProperty);
            EditorGUILayout.PropertyField(m_screenCaptureActionNameProperty);
        #endif
        #endif

            EditorGUILayout.Space(15);
        }


        private void DrawCheckpointsSettings () {
            EditorGUILayout.LabelField("Checkpoints settings", EditorStyles.boldLabel);
            m_playerTagProperty.stringValue = EditorGUILayout.TagField(
                m_playerTagContent, m_playerTagProperty.stringValue
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

    }

}