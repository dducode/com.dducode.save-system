using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal;
using SaveSystem.Security;
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

        public bool Authenticate {
            get => m_handler.Authenticate;
            set => m_handler.Authenticate = value;
        }

        [NotNull]
        public AuthenticationManager AuthManager {
            get => m_handler.AuthManager;
            set => m_handler.AuthManager = value;
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

            return await SaveProfileData(async () => await m_handler.SaveData(dataPath, token), token);
        }


        public async UniTask<HandlingResult> SaveProfileData (
            [NotNull] Stream destination, CancellationToken token = default
        ) {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            return await SaveProfileData(async () => await m_handler.SaveData(destination, token), token);
        }


        [Pure]
        public async UniTask<(HandlingResult, byte[])> SaveProfileData (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                byte[] data = await m_handler.SaveData(token);
                return (HandlingResult.Success, data);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(Name, "Profile saving canceled");
                return (HandlingResult.Canceled, Array.Empty<byte>());
            }
        }


        public async UniTask<HandlingResult> LoadProfileData (
            string dataPath = null, CancellationToken token = default
        ) {
            return await LoadProfileData(async () => await m_handler.LoadData(dataPath ?? DataPath, token), token);
        }


        public async UniTask<HandlingResult> LoadProfileData (
            [NotNull] Stream source, CancellationToken token = default
        ) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return await LoadProfileData(async () => await m_handler.LoadData(source, token), token);
        }


        public async UniTask<HandlingResult> LoadProfileData (
            [NotNull] byte[] data, CancellationToken token = default
        ) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(data));

            return await LoadProfileData(async () => await m_handler.LoadData(data, token), token);
        }


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.PersistentDataPath, m_dataFolder)}";
        }


        private async UniTask<HandlingResult> SaveProfileData (
            Func<UniTask<HandlingResult>> saving, CancellationToken token
        ) {
            return await CancelableOperationsHandler.Execute(saving, Name, "Profile saving canceled", token: token);
        }


        private async UniTask<HandlingResult> LoadProfileData (
            Func<UniTask<HandlingResult>> loading, CancellationToken token
        ) {
            return await CancelableOperationsHandler.Execute(loading, Name, "Profile loading canceled", token: token);
        }

    }

}