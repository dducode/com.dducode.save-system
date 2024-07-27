using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    public sealed class SaveProfile {

        private const string EncryptKey = "profile-encrypt";
        private const string VerifyKey = "profile-verify-checksum";

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
                DataPath = Path.Combine(DataFolder, $"{m_name.ToPathFormat()}.profiledata");
                string oldPath = Path.Combine(DataFolder, $"{oldName}.profiledata");

                if (File.Exists(oldPath))
                    File.Move(oldPath, DataPath);

                SaveSystem.UpdateProfile(this, oldName, m_name);
            }
        }

        public SerializationScope.ScopeSettings Settings => ProfileScope.Settings;
        public DataBuffer SettingsData { get; }
        public DataBuffer Data => ProfileScope.Data;

        [NotNull]
        internal string DataFolder {
            get => m_dataFolder;
            private set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataFolder));

                string newDir = Path.Combine(SaveSystem.ProfilesFolder, value.ToPathFormat());

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
            }
        }

        internal bool HasChanges => Data.HasChanges;

        private string DataPath {
            get => ProfileScope.DataPath;
            set => ProfileScope.DataPath = value;
        }

        private SerializationScope ProfileScope { get; }

        private string m_name;
        private string m_dataFolder;
        private string m_scenesFolder;

        private SceneSerializationContext m_sceneContext;


        internal SaveProfile (string name, DataBuffer settingsData) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            m_name = name;
            ProfileScope = new SerializationScope {
                Name = $"{name} profile scope",
                Settings = {
                    Encrypt = settingsData.Read<bool>(EncryptKey),
                    VerifyChecksum = settingsData.Read<bool>(VerifyKey),
                }
            };

            DataFolder = name;
            DataPath = Path.Combine(DataFolder, $"{name.ToPathFormat()}.profiledata");

            SettingsData = settingsData;
        }


        internal SaveProfile ([NotNull] string name, bool encrypt, bool verify) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            m_name = name;
            ProfileScope = new SerializationScope {
                Name = $"{name} profile scope",
                Settings = {
                    Encrypt = encrypt,
                    VerifyChecksum = verify,
                }
            };

            DataFolder = name;
            DataPath = Path.Combine(DataFolder, $"{name.ToPathFormat()}.profiledata");

            SettingsData = new DataBuffer();
            SettingsData.Write(EncryptKey, encrypt);
            SettingsData.Write(VerifyKey, verify);
        }


        public void CommitChanges () {
            SaveSystem.UpdateProfile(this);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable"/>
        public SaveProfile RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            ProfileScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables"/>
        public SaveProfile RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            ProfileScope.RegisterSerializables(key, serializables);
            return this;
        }


        public async UniTask Save (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await ProfileScope.Serialize(token);
                if (SceneContext != null)
                    await SceneContext.Save(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(ProfileScope.Name, "Data saving canceled");
            }
        }


        public async UniTask Load (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await ProfileScope.Deserialize(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(ProfileScope.Name, "Data loading canceled");
            }
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