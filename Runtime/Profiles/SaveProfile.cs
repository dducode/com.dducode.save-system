using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Providers;
using SaveSystemPackage.SerializableData;
using SaveSystemPackage.Storages;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage.Profiles {

    public sealed class SaveProfile : SerializationScope {

        public XmlDictionary dictionary;

        [NotNull]
        public override string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));
                if (string.Equals(m_name, value))
                    return;

                SaveSystem.ProfilesManager.ThrowIfProfileExistsWithName(value);
                m_name = value;
                SaveSystem.ProfilesManager.UpdateProfile(this);
            }
        }

        public string IconId {
            get => m_iconId;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(IconId));
                if (string.Equals(m_name, value))
                    return;

                m_iconId = value;
                SaveSystem.ProfilesManager.UpdateProfile(this);
            }
        }

        public string Id { get; }

        [NotNull]
        internal SceneSerializationScope SceneScope {
            get => m_sceneScope;
            set => m_sceneScope = value ?? throw new ArgumentNullException(nameof(SceneScope));
        }

        private string m_name;
        private string m_iconId;
        private SceneSerializationScope m_sceneScope;


        internal SaveProfile (ProfileData data) {
            Id = data.id;
            m_name = data.name;
            m_iconId = data.iconId;
            directory = Storage.ProfilesDirectory.GetOrCreateDirectory(Id);
            KeyProvider = new CompositeKeyStore(SaveSystem.Game.KeyProvider, directory.Name);
            DataStorage = new FileSystemStorage(directory, SaveSystem.Settings.Serializer.GetFormatCode());
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


        public ProfileData GetData () {
            return new ProfileData {
                id = Id,
                name = Name,
                iconId = IconId
            };
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

    }

}