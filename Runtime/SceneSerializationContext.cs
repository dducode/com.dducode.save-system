using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal;
using SaveSystem.Security;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystem {

    public sealed class SceneSerializationContext : MonoBehaviour {

        [SerializeField]
        private bool encrypt;

        [SerializeField]
        private bool authentication;

        [SerializeField]
        private string fileName;

        public bool Encrypt {
            get => m_handler.Encrypt;
            set => m_handler.Encrypt = value;
        }

        /// <summary>
        /// Cryptographer used to encrypt/decrypt serializable data
        /// </summary>
        [NotNull]
        public Cryptographer Cryptographer {
            get => m_handler.Cryptographer;
            set => m_handler.Cryptographer = value;
        }

        public bool Authenticate {
            get => m_handler.Authenticate;
            set => m_handler.Authenticate = value;
        }

        [NotNull]
        public AuthenticationManager AuthManager {
            get => m_handler.AuthManager;
            set => m_handler.AuthManager = value;
        }

        private string DataPath {
            get {
                SaveProfile profile = SaveSystemCore.SelectedSaveProfile;
                return Path.Combine(
                    profile == null ? SaveSystemCore.ScenesFolder : profile.DataFolder, fileName
                );
            }
        }

        private SaveDataHandler m_handler;
        private SerializationScope m_serializationScope;


        private void Awake () {
            m_handler = new SaveDataHandler {
                SerializationScope = m_serializationScope = new SerializationScope {
                    Name = $"{name} scope"
                }
            };

            Encrypt = encrypt;
            Authenticate = authentication;

            SaveSystemSettings settings = ResourcesManager.LoadSettings();

            if (Encrypt)
                Cryptographer = new Cryptographer(settings.encryptionSettings);

            if (Authenticate)
                AuthManager = new AuthenticationManager(settings.authenticationSettings);
        }


        private void OnValidate () {
            if (string.IsNullOrEmpty(fileName))
                fileName = $"{gameObject.scene.name}.scenedata";
        }


        public void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_serializationScope.WriteData(key, value);
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key, TValue defaultValue = default) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_serializationScope.ReadData(key, defaultValue);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IRuntimeSerializable)"/>
        public SceneSerializationContext RegisterSerializable (
            [NotNull] string key, [NotNull] IRuntimeSerializable serializable
        ) {
            m_serializationScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IRuntimeSerializable})"/>
        public SceneSerializationContext RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            m_serializationScope.RegisterSerializables(key, serializables);
            return this;
        }


        public async UniTask SaveSceneData (CancellationToken token = default) {
            await CancelableOperationsHandler.Execute(
                async () => await m_handler.SaveData(DataPath, token),
                name, "Scene data saving canceled", this, token
            );
        }


        public async UniTask LoadSceneData (CancellationToken token = default) {
            await CancelableOperationsHandler.Execute(
                async () => await m_handler.LoadData(DataPath, token),
                name, "Scene data loading canceled", this, token
            );
        }

    }

}