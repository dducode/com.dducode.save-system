using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Security;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

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

                SaveSystem.UpdateProfile(this, oldName, m_name);
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

                string newDir = Path.Combine(SaveSystem.ProfilesFolder, value.ToLower().Replace(' ', '-'));

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

            using SaveSystemSettings settings = ResourcesManager.LoadSettings();

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


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.StorageDataPath, DataPath)}";
        }


        internal async UniTask<byte[]> ExportProfileData (CancellationToken token = default) {
            string[] entries = Directory.GetFileSystemEntries(DataFolder);
            if (entries.Length == 0)
                return Array.Empty<byte>();

            using var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);

            writer.Write(entries.Length);

            foreach (string path in entries) {
                writer.Write(path);
                writer.Write(await File.ReadAllBytesAsync(path, token));
            }

            return memoryStream.ToArray();
        }


        internal async UniTask ImportProfileData (byte[] data, CancellationToken token = default) {
            if (data.Length == 0)
                return;

            await using var reader = new SaveReader(new MemoryStream(data));
            var entriesCount = reader.Read<int>();

            for (var i = 0; i < entriesCount; i++)
                await File.WriteAllBytesAsync(reader.ReadString(), reader.ReadArray<byte>(), token);
        }


        internal void Clear () {
            ProfileScope.Clear();
        }

    }

}