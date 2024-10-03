#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SaveSystemPackage.ComponentsRecording;
using SaveSystemPackage.Compressing;
using SaveSystemPackage.Profiles;
using SaveSystemPackage.Providers;
using SaveSystemPackage.Security;
using SaveSystemPackage.Serialization;
using UnityEngine;
using Logger = SaveSystemPackage.Internal.Logger;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once InconsistentNaming

namespace SaveSystemPackage {

    public static partial class SaveSystem {

        public class SystemSettings {

            /// <summary>
            /// It's used to enable logs
            /// </summary>
            /// <example> EnabledLogs = LogLevel.Warning | LogLevel.Error </example>
            /// <seealso cref="LogLevel"/>
            public LogLevel EnabledLogs {
                get => Logger.EnabledLogs;
                set => Logger.EnabledLogs = value;
            }

            /// <summary>
            /// It's used to manage autosave loop, save on focus changed, on low memory and on quitting the game
            /// </summary>
            /// <example> EnabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnFocusChanged </example>
            /// <seealso cref="SaveEvents"/>
            public SaveEvents EnabledSaveEvents {
                get => m_enabledSaveEvents;
                set => SetupEvents(m_enabledSaveEvents = value);
            }

            /// <summary>
            /// It's used to determine periodic saving frequency
            /// </summary>
            /// <value> Saving period in seconds </value>
            /// <remarks> If it equals 0, saving will be executed at every frame </remarks>
            public float SavePeriod {
                get => m_savePeriod;
                set {
                    if (value < 0) {
                        throw new ArgumentException(
                            "Save period cannot be less than 0.", nameof(SavePeriod)
                        );
                    }

                    m_savePeriod = value;
                }
            }

            /// <summary>
            /// Player tag is used to filtering messages from triggered checkpoints
            /// </summary>
            /// <value> Tag of the player object </value>
            [NotNull]
            public string PlayerTag {
                get => m_playerTag;
                set {
                    if (string.IsNullOrEmpty(value)) {
                        throw new ArgumentNullException(
                            nameof(PlayerTag), "Player tag cannot be null or empty"
                        );
                    }

                    m_playerTag = value;
                }
            }

        #if ENABLE_LEGACY_INPUT_MANAGER
            /// <summary>
            /// Binds any key to quick save
            /// </summary>
            public KeyCode QuickSaveKey {
                get => m_quickSaveKey;
                set {
                    m_quickSaveKey = value;
                    PlayerPrefs.SetInt(SaveSystemConstants.QuickSaveKeyCode, (int)m_quickSaveKey);
                }
            }

            /// <summary>
            /// Binds any key to screen capture
            /// </summary>
            public KeyCode ScreenCaptureKey {
                get => m_screenCaptureKey;
                set {
                    m_screenCaptureKey = value;
                    PlayerPrefs.SetInt(SaveSystemConstants.ScreenCaptureKeyCode, (int)m_screenCaptureKey);
                }
            }
        #endif

        #if ENABLE_INPUT_SYSTEM
            /// <summary>
            /// Binds any input action to quick save
            /// </summary>
            public InputAction QuickSaveAction { get; set; }

            /// <summary>
            /// Binds any input action to screen capture
            /// </summary>
            public InputAction ScreenCaptureAction { get; set; }
        #endif

            public ISerializer Serializer {
                get => m_serializer;
                set => m_serializer = value ?? throw new ArgumentNullException(nameof(Serializer));
            }

            public Dictionary<Type, string> KeyMap { get; } = new() {
                {typeof(TransformData), "transform-data"},
                {typeof(RigidbodyData), "rigidbody-data"},
                {typeof(MeshData), "mesh-data"},
                {typeof(ProfileData), "profile-data"},
                {typeof(ProfilesManagerData), "profiles-manager-data"}
            };


        #if ENABLE_BOTH_SYSTEMS
            public UsedInputSystem UsedInputSystem { get; private set; }
        #endif

            private SaveEvents m_enabledSaveEvents;
            private float m_savePeriod;
            private string m_playerTag;

        #if ENABLE_LEGACY_INPUT_MANAGER
            private KeyCode m_screenCaptureKey;
            private KeyCode m_quickSaveKey;
        #endif

            private ISerializer m_serializer;
            private IKeyProvider m_keyProvider;


            public static implicit operator SystemSettings (SaveSystemSettings settings) {
                return new SystemSettings(settings);
            }


            private SystemSettings (SaveSystemSettings settings) {
                EnabledLogs = settings.enabledLogs;
                EnabledSaveEvents = settings.enabledSaveEvents;
                SavePeriod = settings.savePeriod;
                PlayerTag = settings.playerTag;

                SetupUserInputs(settings);
                Serializer = SelectSerializer(settings);
            }


            private void SetupUserInputs (SaveSystemSettings settings) {
            #if ENABLE_BOTH_SYSTEMS
                UsedInputSystem = settings.usedInputSystem;

                switch (UsedInputSystem) {
                    case UsedInputSystem.LegacyInputManager:
                        QuickSaveKey = (KeyCode)PlayerPrefs.GetInt(
                            SaveSystemConstants.QuickSaveKeyCode, (int)settings.quickSaveKey
                        );
                        ScreenCaptureKey = (KeyCode)PlayerPrefs.GetInt(
                            SaveSystemConstants.ScreenCaptureKeyCode, (int)settings.screenCaptureKey
                        );
                        break;
                    case UsedInputSystem.InputSystem:
                        QuickSaveAction = settings.quickSaveAction;
                        QuickSaveAction?.Enable();
                        ScreenCaptureAction = settings.screenCaptureAction;
                        ScreenCaptureAction?.Enable();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            #else
            #if ENABLE_LEGACY_INPUT_MANAGER
                QuickSaveKey = settings.quickSaveKey;
                ScreenCaptureKey = settings.screenCaptureKey;
            #endif

            #if ENABLE_INPUT_SYSTEM
                QuickSaveAction = settings.quickSaveAction;
                QuickSaveAction?.Enable();
                ScreenCaptureAction = settings.screenCaptureAction;
                ScreenCaptureAction?.Enable();
            #endif
            #endif
            }


            private ISerializer SelectSerializer (SaveSystemSettings settings) {
                SerializerType serializerType = settings.serializerType;

                switch (serializerType) {
                    case SerializerType.BinarySerializer:
                        return new BinarySerializer();
                    case SerializerType.JsonSerializer:
                        return new JsonSerializer();
                    case SerializerType.EncryptionSerializer:
                        var cryptographer = new Cryptographer(settings.encryptionSettings);
                        ISerializer baseSerializer = SelectBaseSerializer(settings.baseSerializerType);
                        return new EncryptionSerializer(baseSerializer, cryptographer);
                    case SerializerType.CompressionSerializer:
                        var compressor = new FileCompressor(settings.compressionSettings);
                        baseSerializer = SelectBaseSerializer(settings.baseSerializerType);
                        return new CompressionSerializer(baseSerializer, compressor);
                    case SerializerType.CompositeSerializer:
                        cryptographer = new Cryptographer(settings.encryptionSettings);
                        compressor = new FileCompressor(settings.compressionSettings);
                        baseSerializer = SelectBaseSerializer(settings.baseSerializerType);
                        return new CompositeSerializer(baseSerializer, cryptographer, compressor);
                    case SerializerType.Custom:
                        return null;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serializerType), serializerType, null);
                }
            }


            private ISerializer SelectBaseSerializer (BaseSerializerType serializerType) {
                switch (serializerType) {
                    case BaseSerializerType.BinarySerializer:
                        return new BinarySerializer();
                    case BaseSerializerType.JsonSerializer:
                        return new JsonSerializer();
                    case BaseSerializerType.Custom:
                        return null;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serializerType), serializerType, null);
                }
            }

        }

    }

}