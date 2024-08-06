using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Security;
using SaveSystemPackage.Serialization;
using Directory = SaveSystemPackage.Internal.Directory;
using File = SaveSystemPackage.Internal.File;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    public abstract class SaveProfile {

        [NotNull]
        public string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                if (string.Equals(m_name, value))
                    return;

                SaveSystem.ThrowIfProfileExistsWithName(value);
                string oldName = m_name;
                m_name = value;
                ProfileScope.Name = $"{value} scope";

                SaveSystem.UpdateProfile(this, oldName, m_name);
            }
        }

        public SerializationSettings Settings => ProfileScope.Settings;
        public DataBuffer Data => ProfileScope.Data;
        public SecureDataBuffer SecureData => ProfileScope.SecureData;

        [NotNull]
        internal Directory DataDirectory {
            get => m_dataDirectory;
            set => m_dataDirectory = value ?? throw new ArgumentNullException(nameof(DataDirectory));
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

        internal File DataFile {
            get => ProfileScope.DataFile;
            set => ProfileScope.DataFile = value;
        }

        private SerializationScope ProfileScope { get; set; }

        private string m_name;
        private Directory m_dataDirectory;
        private string m_scenesFolder;

        private SceneSerializationContext m_sceneContext;


        internal void Initialize ([NotNull] string name, bool? encrypt, bool? compressFiles) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            m_name = name;

            using (SaveSystemSettings settings = SaveSystemSettings.Load()) {
                ProfileScope = new SerializationScope {
                    Name = $"{name} profile scope",
                    Settings = {
                        Encrypt = encrypt ?? settings.encrypt,
                        CompressFiles = compressFiles ?? settings.compressFiles
                    }
                };
            }

            OnInitialized();
        }


        public void ApplyChanges () {
            SaveSystem.UpdateProfile(this);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,IRuntimeSerializable)"/>
        public SaveProfile RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            ProfileScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,object)"/>
        public SaveProfile RegisterSerializable ([NotNull] string key, [NotNull] object obj) {
            ProfileScope.RegisterSerializable(key, obj);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables"/>
        public SaveProfile RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            ProfileScope.RegisterSerializables(key, serializables);
            return this;
        }


        public async Task Save (CancellationToken token = default) {
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


        public async Task Load (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await ProfileScope.Deserialize(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(ProfileScope.Name, "Data loading canceled");
            }
        }


        public override string ToString () {
            return $"name: {Name}, path: {DataFile.FullName}";
        }


        internal async Task<byte[]> ExportProfileData (CancellationToken token = default) {
            File[] entries = DataDirectory.EnumerateFiles().ToArray();
            if (entries.Length == 0)
                return Array.Empty<byte>();

            using var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);

            writer.Write(entries.Length);

            foreach (File file in entries) {
                writer.Write(file.Name);
                writer.Write(file.Extension);
                writer.Write(await file.ReadAllBytesAsync(token));
            }

            return memoryStream.ToArray();
        }


        internal async Task ImportProfileData (byte[] data, CancellationToken token = default) {
            if (data.Length == 0)
                return;

            await using var reader = new SaveReader(new MemoryStream(data));
            var entriesCount = reader.Read<int>();

            for (var i = 0; i < entriesCount; i++) {
                await DataDirectory
                   .GetOrCreateFile(reader.ReadString(), reader.ReadString())
                   .WriteAllBytesAsync(reader.ReadArray<byte>(), token);
            }
        }


        internal void Clear () {
            ProfileScope.Clear();
        }


        protected virtual void OnInitialized () { }

    }

}