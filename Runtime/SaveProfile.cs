using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using Directory = SaveSystemPackage.Internal.Directory;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    public abstract class SaveProfile : SerializationScope {

        [NotNull]
        public override string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                if (string.Equals(m_name, value))
                    return;

                SaveSystem.ThrowIfProfileExistsWithName(value);
                string oldName = m_name;
                m_name = value;

                SaveSystem.UpdateProfile(this, oldName, m_name);
            }
        }

        [NotNull]
        internal Directory DataDirectory {
            get => m_dataDirectory;
            set => m_dataDirectory = value ?? throw new ArgumentNullException(nameof(DataDirectory));
        }

        [NotNull]
        internal SceneSerializationScope SceneScope {
            get => m_sceneScope;
            set => m_sceneScope = value ?? throw new ArgumentNullException(nameof(SceneScope));
        }

        internal bool HasChanges => Data.HasChanges || SecureData.HasChanges;

        private string m_name;
        private Directory m_dataDirectory;
        private string m_scenesFolder;

        private SceneSerializationScope m_sceneScope;


        internal void Initialize ([NotNull] string name) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            m_name = name;
            OnInitialized();
        }


        public void ApplyChanges () {
            SaveSystem.UpdateProfile(this);
        }


        public override string ToString () {
            return $"name: {Name}";
        }


        internal async Task Save (SaveType saveType, CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                await OnSaveInvoke(saveType);
                if (SceneScope != null)
                    await SceneScope.Save(saveType, token);
            }
            catch (OperationCanceledException) {
                Logger.Log(Name, "Data saving canceled");
            }
        }


        public async Task Reload (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await OnReloadInvoke();
                if (SceneScope != null)
                    await SceneScope.Reload(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(Name, "Data reload canceled");
            }
        }


        // internal async Task<byte[]> ExportProfileData (CancellationToken token) {
        //     File[] entries = DataDirectory.EnumerateFiles().ToArray();
        //     if (entries.Length == 0)
        //         return Array.Empty<byte>();
        //
        //     using var memoryStream = new MemoryStream();
        //     await using var writer = new SaveWriter(memoryStream);
        //
        //     writer.Write(entries.Length);
        //
        //     foreach (File file in entries) {
        //         writer.Write(file.Name);
        //         writer.Write(file.Extension);
        //         writer.Write(await file.ReadAllBytesAsync(token));
        //     }
        //
        //     return memoryStream.ToArray();
        // }


        // internal async Task ImportProfileData (byte[] data, CancellationToken token) {
        //     if (data.Length == 0)
        //         return;
        //
        //     await using var reader = new SaveReader(new MemoryStream(data));
        //     var entriesCount = reader.Read<int>();
        //
        //     for (var i = 0; i < entriesCount; i++) {
        //         await DataDirectory
        //            .GetOrCreateFile(reader.ReadString(), reader.ReadString())
        //            .WriteAllBytesAsync(reader.ReadArray<byte>(), token);
        //     }
        // }


        protected virtual void OnInitialized () { }

    }

}