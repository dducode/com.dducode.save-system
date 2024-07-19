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

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem {

    public abstract class SaveProfile : IRuntimeSerializable {

        public virtual int Version => 0;

        [NotNull]
        public string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                if (string.Equals(m_name, value))
                    return;

                string oldName = m_name;
                m_name = value;
                m_serializationScope.Name = $"{value} scope";

                DataFolder = m_name;
                string sourceFile = Path.Combine(DataFolder, $"{oldName}.profiledata");

                if (File.Exists(sourceFile))
                    File.Move(sourceFile, Path.Combine(DataFolder, $"{m_name}.profiledata"));

                SaveSystemCore.UpdateProfile(this, oldName, m_name);
            }
        }

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

        [NotNull]
        internal string DataFolder {
            get => m_dataFolder;
            private set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataFolder));

                string newDir = Path.Combine(SaveSystemCore.ProfilesFolder, value);

                if (string.Equals(m_dataFolder, newDir))
                    return;

                if (Directory.Exists(m_dataFolder))
                    Directory.Move(m_dataFolder, newDir);
                else
                    Directory.CreateDirectory(newDir);

                m_dataFolder = newDir;
            }
        }

        internal string DataPath => Path.Combine(DataFolder, $"{m_name}.profiledata");

        private string m_name;
        private string m_dataFolder;
        private readonly SerializationScope m_serializationScope;
        private readonly SaveDataHandler m_handler;


        protected SaveProfile () {
            m_handler = new SaveDataHandler {
                SerializationScope = m_serializationScope = new SerializationScope()
            };
        }


        protected SaveProfile (string name) : this() {
            m_name = name;
            DataFolder = name;
        }


        public virtual void Serialize (SaveWriter writer) {
            writer.Write(Name);
            writer.Write(Encrypt);
            writer.Write(Authenticate);
        }


        public virtual void Deserialize (SaveReader reader, int previousVersion) {
            m_name = reader.ReadString();
            m_serializationScope.Name = $"{m_name} scope";
            DataFolder = m_name;

            var settings = ResourcesManager.LoadSettings();

            Encrypt = reader.Read<bool>();

            if (Encrypt)
                Cryptographer = new Cryptographer(settings.encryptionSettings);

            Authenticate = reader.Read<bool>();

            if (Authenticate)
                AuthManager = new AuthenticationManager(settings.authenticationSettings);
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
        public void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            m_serializationScope.RegisterSerializable(key, serializable);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IRuntimeSerializable})"/>
        public void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            m_serializationScope.RegisterSerializables(key, serializables);
        }


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.StorageDataPath, DataPath)}";
        }


        internal async UniTask SaveProfileData (CancellationToken token = default) {
            await CancelableOperationsHandler.Execute(
                async () => await m_handler.SaveData(DataPath, token),
                Name, "Profile saving canceled", token: token
            );
        }


        internal async UniTask LoadProfileData (CancellationToken token = default) {
            await CancelableOperationsHandler.Execute(
                async () => await m_handler.LoadData(DataPath, token),
                Name, "Profile loading canceled", token: token
            );
        }


        internal async UniTask<byte[]> ExportProfileData (CancellationToken token = default) {
            if (!File.Exists(DataPath))
                return Array.Empty<byte>();
            return await File.ReadAllBytesAsync(DataPath, token);
        }


        internal async UniTask ImportProfileData (byte[] data, CancellationToken token = default) {
            if (data.Length > 0)
                await File.WriteAllBytesAsync(DataPath, data, token);
        }


        internal void Clear () {
            m_serializationScope.Clear();
        }

    }

}