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

        public bool Authenticate {
            get => m_handler.Authenticate;
            set => m_handler.Authenticate = value;
        }

        [NotNull]
        public AuthenticationManager AuthManager {
            get => m_handler.AuthManager;
            set => m_handler.AuthManager = value;
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
                Authenticate = authentication,
                AuthManager = new AuthenticationManager(authHashKey, algorithmName),
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

            return await SaveSceneData(async () => await m_handler.SaveData(dataPath, token), token);
        }


        public async UniTask<HandlingResult> SaveSceneData (
            [NotNull] Stream destination, CancellationToken token = default
        ) {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            return await SaveSceneData(async () => await m_handler.SaveData(destination, token), token);
        }


        [Pure]
        public async UniTask<(HandlingResult, byte[])> SaveSceneData (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                byte[] data = await m_handler.SaveData(token);
                return (HandlingResult.Success, data);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data saving canceled", this);
                return (HandlingResult.Canceled, Array.Empty<byte>());
            }
        }


        public async UniTask<HandlingResult> LoadSceneData (
            string dataPath = null, CancellationToken token = default
        ) {
            return await LoadSceneData(async () => await m_handler.LoadData(dataPath ?? DataPath, token), token);
        }


        public async UniTask<HandlingResult> LoadSceneData (
            [NotNull] Stream source, CancellationToken token = default
        ) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return await LoadSceneData(async () => await m_handler.LoadData(source, token), token);
        }


        public async UniTask<HandlingResult> LoadSceneData ([NotNull] byte[] data, CancellationToken token = default) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            return await LoadSceneData(async () => await m_handler.LoadData(data, token), token);
        }


        private async UniTask<HandlingResult> SaveSceneData (
            Func<UniTask<HandlingResult>> saving, CancellationToken token
        ) {
            return await CancelableOperationsHandler.Execute(saving, name, "Scene data saving canceled", this, token);
        }


        private async UniTask<HandlingResult> LoadSceneData (
            Func<UniTask<HandlingResult>> loading, CancellationToken token
        ) {
            return await CancelableOperationsHandler.Execute(loading, name, "Scene data loading canceled", this, token);
        }

    }

}