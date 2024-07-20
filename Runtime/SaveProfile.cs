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
                ProfileScope.Name = $"{value} scope";

                DataFolder = m_name;
                DataPath = Path.Combine(DataFolder, $"{m_name.ToLower().Replace(' ', '-')}.profiledata");
                string oldPath = Path.Combine(DataFolder, $"{oldName}.profiledata");

                if (File.Exists(oldPath))
                    File.Move(oldPath, DataPath);

                SaveSystemCore.UpdateProfile(this, oldName, m_name);
            }
        }


        public bool Encrypt {
            get => ProfileScope.Encrypt;
            set => ProfileScope.Encrypt = value;
        }

        [NotNull]
        public Cryptographer Cryptographer {
            get => ProfileScope.Cryptographer;
            set => ProfileScope.Cryptographer = value;
        }

        public bool Authenticate {
            get => ProfileScope.Authenticate;
            set => ProfileScope.Authenticate = value;
        }

        [NotNull]
        public AuthenticationManager AuthManager {
            get => ProfileScope.AuthManager;
            set => ProfileScope.AuthManager = value;
        }

        [NotNull]
        internal string DataFolder {
            get => m_dataFolder;
            private set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataFolder));

                string newDir = Path.Combine(SaveSystemCore.ProfilesFolder, value.ToLower().Replace(' ', '-'));

                if (string.Equals(m_dataFolder, newDir))
                    return;

                if (Directory.Exists(m_dataFolder))
                    Directory.Move(m_dataFolder, newDir);
                else
                    Directory.CreateDirectory(newDir);

                m_dataFolder = newDir;
            }
        }

        [NotNull]
        internal string ScenesFolder {
            get {
                if (string.IsNullOrEmpty(m_scenesFolder)) {
                    m_scenesFolder = Path.Combine(DataFolder, "_scenes");
                    Directory.CreateDirectory(m_scenesFolder);
                }

                return m_scenesFolder;
            }
        }

        [NotNull]
        internal SceneSerializationContext SceneContext {
            get => m_sceneContext;
            set {
                if (value == null)
                    throw new ArgumentNullException(nameof(SceneContext));
                m_sceneContext = value;
                ProfileScope.AttachNestedScope(m_sceneContext.SceneScope);
            }
        }

        internal SerializationScope ProfileScope { get; }

        private string DataPath {
            get => ProfileScope.DataPath;
            set => ProfileScope.DataPath = value;
        }

        private string m_name;
        private string m_dataFolder;
        private string m_scenesFolder;
        private SceneSerializationContext m_sceneContext;


        protected SaveProfile () {
            ProfileScope = new SerializationScope();
        }


        protected SaveProfile (string name) : this() {
            m_name = name;
            DataFolder = name;
            DataPath = Path.Combine(DataFolder, $"{name.ToLower().Replace(' ', '-')}.profiledata");
        }


        public virtual void Serialize (SaveWriter writer) {
            writer.Write(Name);
            writer.Write(Encrypt);
            writer.Write(Authenticate);
        }


        public virtual void Deserialize (SaveReader reader, int previousVersion) {
            m_name = reader.ReadString();
            ProfileScope.Name = $"{m_name} scope";
            DataFolder = m_name;
            DataPath = Path.Combine(DataFolder, $"{m_name.ToLower().Replace(' ', '-')}.profiledata");

            SaveSystemSettings settings = ResourcesManager.LoadSettings();

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

            ProfileScope.WriteData(key, value);
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key, TValue defaultValue = default) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return ProfileScope.ReadData(key, defaultValue);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IRuntimeSerializable)"/>
        public void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            ProfileScope.RegisterSerializable(key, serializable);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IRuntimeSerializable})"/>
        public void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            ProfileScope.RegisterSerializables(key, serializables);
        }


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.StorageDataPath, DataPath)}";
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
            ProfileScope.Clear();
        }

    }

}