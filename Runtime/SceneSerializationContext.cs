using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Cryptography;
using SaveSystem.Internal;
using UnityEngine;
using Logger = SaveSystem.Internal.Logger;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystem {

    public sealed class SceneSerializationContext : MonoBehaviour {

        [SerializeField]
        private bool encrypt;

        [SerializeField]
        private EncryptionSettings encryptionSettings;

        [SerializeField]
        private bool authentication;

        [SerializeField]
        private HashAlgorithmName algorithmName;

        [SerializeField]
        private string authHashKey = Guid.NewGuid().ToString();

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

        public bool Authentication {
            get => m_handler.Authentication;
            set => m_handler.Authentication = value;
        }

        public HashAlgorithmName AlgorithmName {
            get => m_handler.AlgorithmName;
            set => m_handler.AlgorithmName = value;
        }

        [NotNull]
        public string AuthHashKey {
            get => m_handler.AuthHashKey;
            set => m_handler.AuthHashKey = value;
        }

        public string DataPath => Path.Combine(
            SaveSystemCore.SelectedSaveProfile.DataFolder, $"{gameObject.scene.name}.scenedata"
        );

        private SaveDataHandler m_handler;
        private SerializationScope m_serializationScope;


        private void Awake () {
            m_handler = new SaveDataHandler {
                SerializationScope = m_serializationScope = new SerializationScope {
                    Name = $"{name} scope"
                },
                Authentication = authentication,
                AlgorithmName = algorithmName,
                AuthHashKey = authHashKey,
                Encrypt = encrypt
            };

            if (encryptionSettings != null)
                Cryptographer = new Cryptographer(encryptionSettings);
            else if (Encrypt)
                Logger.LogError(name, "Encryption enabled but settings not set");
        }


        public void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_serializationScope.WriteData(key, value);
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_serializationScope.ReadData<TValue>(key);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IRuntimeSerializable)"/>
        public SceneSerializationContext RegisterSerializable (
            [NotNull] string key, [NotNull] IRuntimeSerializable serializable
        ) {
            m_serializationScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IAsyncRuntimeSerializable)"/>
        public SceneSerializationContext RegisterSerializable (
            [NotNull] string key, [NotNull] IAsyncRuntimeSerializable serializable
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


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IAsyncRuntimeSerializable})"/>
        public SceneSerializationContext RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables
        ) {
            m_serializationScope.RegisterSerializables(key, serializables);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.ObserveProgress(IProgress{float})"/>
        public void ObserveProgress ([NotNull] IProgress<float> progress) {
            m_serializationScope.ObserveProgress(progress);
        }


        /// <inheritdoc cref="SerializationScope.ObserveProgress(IProgress{float}, IProgress{float})"/>
        public void ObserveProgress (
            [NotNull] IProgress<float> saveProgress, [NotNull] IProgress<float> loadProgress
        ) {
            m_serializationScope.ObserveProgress(saveProgress, loadProgress);
        }


        public async UniTask<HandlingResult> SaveSceneData (
            [NotNull] string dataPath, CancellationToken token = default
        ) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.SaveData(dataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data saving canceled", this);
                return HandlingResult.Canceled;
            }
        }


        [Pure]
        public async UniTask<(HandlingResult, byte[])> SaveSceneData (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.SaveData(token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data saving canceled", this);
                return (HandlingResult.Canceled, Array.Empty<byte>());
            }
        }


        public async UniTask<HandlingResult> LoadSceneData ([NotNull] byte[] data, CancellationToken token = default) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.LoadData(data, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data loading canceled", this);
                return HandlingResult.Canceled;
            }
        }


        public async UniTask<HandlingResult> LoadSceneData (
            string dataPath = null, CancellationToken token = default
        ) {
            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.LoadData(dataPath ?? DataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data loading canceled", this);
                return HandlingResult.Canceled;
            }
        }

    }

}