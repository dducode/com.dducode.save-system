using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Cryptography;
using SaveSystem.Internal;
using Logger = SaveSystem.Internal.Logger;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem {

    public abstract class SaveProfile : IRuntimeSerializable {

        [NotNull]
        public string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                m_name = value;
                m_serializationScope.Name = $"{value} scope";
                m_handler.AuthHashKey = $"{value} key";
            }
        }

        [NotNull]
        public string DataFolder {
            get => m_dataFolder;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataFolder));

                m_dataFolder = Storage.PrepareBeforeUsing(value, true);
            }
        }

        public string DataPath => Path.Combine(m_dataFolder, $"{m_name}.profiledata");

        public bool Encrypt {
            get => m_handler.Encrypt;
            set => m_handler.Encrypt = value;
        }

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

        private string m_name;
        private string m_dataFolder;
        private readonly SerializationScope m_serializationScope;
        private readonly SaveDataHandler m_handler;


        protected SaveProfile () {
            m_handler = new SaveDataHandler {
                SerializationScope = m_serializationScope = new SerializationScope()
            };
        }


        public virtual void Serialize (SaveWriter writer) {
            writer.Write(Name);
            writer.Write(DataFolder);
        }


        public virtual void Deserialize (SaveReader reader) {
            Name = reader.ReadString();
            DataFolder = reader.ReadString();
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
        public void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            m_serializationScope.RegisterSerializable(key, serializable);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IAsyncRuntimeSerializable)"/>
        public void RegisterSerializable ([NotNull] string key, [NotNull] IAsyncRuntimeSerializable serializable) {
            m_serializationScope.RegisterSerializable(key, serializable);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IRuntimeSerializable})"/>
        public void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            m_serializationScope.RegisterSerializables(key, serializables);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IAsyncRuntimeSerializable})"/>
        public void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables
        ) {
            m_serializationScope.RegisterSerializables(key, serializables);
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


        public async UniTask<HandlingResult> SaveProfileData (
            [NotNull] string dataPath, CancellationToken token = default
        ) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.SaveData(dataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(Name, "Profile saving canceled");
                return HandlingResult.Canceled;
            }
        }


        [Pure]
        public async UniTask<(HandlingResult, byte[])> SaveProfileData (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.SaveData(token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(Name, "Profile saving canceled");
                return (HandlingResult.Canceled, Array.Empty<byte>());
            }
        }


        public async UniTask<HandlingResult> LoadProfileData (
            [NotNull] byte[] data, CancellationToken token = default
        ) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.LoadData(data, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(Name, "Profile loading canceled");
                return HandlingResult.Canceled;
            }
        }


        public async UniTask<HandlingResult> LoadProfileData (
            string dataPath = null, CancellationToken token = default
        ) {
            try {
                token.ThrowIfCancellationRequested();
                return await m_handler.LoadData(dataPath ?? DataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(Name, "Profile loading canceled");
                return HandlingResult.Canceled;
            }
        }


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.PersistentDataPath, m_dataFolder)}";
        }

    }

}