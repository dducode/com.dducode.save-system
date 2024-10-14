using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using SaveSystemPackage.SerializableData;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage.Profiles {

    public sealed class SaveProfile : SerializationContext {

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
        internal SceneSerializationContext SceneContext {
            get => m_sceneContext;
            set => m_sceneContext = value ?? throw new ArgumentNullException(nameof(SceneContext));
        }

        private string m_name;
        private string m_iconId;
        private SceneSerializationContext m_sceneContext;


        internal SaveProfile (ProfileData data, Directory directory) {
            Id = data.id;
            m_name = data.name;
            m_iconId = data.iconId;
            this.directory = directory;
        }


        public async Task Reload (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await OnReloadInvoke();
                if (SceneContext != null)
                    await SceneContext.Reload(token);
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
                if (SceneContext != null)
                    await SceneContext.Save(saveType, token);
            }
            catch (OperationCanceledException) {
                Logger.Log(Name, "Data saving canceled");
            }
        }

    }

}