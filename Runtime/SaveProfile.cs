using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Security;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    public sealed class SaveProfile {

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

        public bool AutoSave { get; set; }

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

        private SerializationScope ProfileScope { get; }

        internal string DataPath {
            get => ProfileScope.DataPath;
            private set => ProfileScope.DataPath = value;
        }

        private string m_name;
        private string m_dataFolder;
        private string m_scenesFolder;
        private SceneSerializationContext m_sceneContext;


        internal SaveProfile ([NotNull] string name, bool autoSave, bool encrypt, bool authenticate) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            m_name = name;
            ProfileScope = new SerializationScope {
                Name = $"{m_name} profile scope"
            };
            DataFolder = name;
            DataPath = Path.Combine(DataFolder, $"{name.ToPathFormat()}.profiledata");

            AutoSave = autoSave;
            Encrypt = encrypt;
            Authenticate = authenticate;
        }


        public void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            ProfileScope.WriteData(key, value);

            if (AutoSave)
                ScheduleAutoSave();
        }


        public void WriteData<TValue> ([NotNull] string key, [NotNull] TValue[] array) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            ProfileScope.WriteData(key, array);

            if (AutoSave)
                ScheduleAutoSave();
        }


        public void WriteData ([NotNull] string key, [NotNull] string value) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            ProfileScope.WriteData(key, value);

            if (AutoSave)
                ScheduleAutoSave();
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key, TValue defaultValue = default) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return ProfileScope.ReadData(key, defaultValue);
        }


        [Pure]
        public TValue[] ReadArray<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return ProfileScope.ReadArray<TValue>(key);
        }


        [Pure]
        public string ReadData ([NotNull] string key, string defaultValue = null) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return ProfileScope.ReadData(key, defaultValue);
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


        private void ScheduleAutoSave () {
            SaveSystem.SynchronizationPoint.ScheduleTask(async token => await ProfileScope.Serialize(token));
        }

    }

}