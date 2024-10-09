using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Profiles;
using SaveSystemPackage.Providers;
using SaveSystemPackage.Storages;

// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage {

    public sealed class Game : SerializationScope {

        /// <summary>
        /// Uses for serializing data into a separately file
        /// </summary>
        [NotNull]
        public SaveProfile SaveProfile {
            get => m_saveProfile;
            set {
                m_saveProfile = value ?? throw new ArgumentNullException(nameof(SaveProfile));
                if (m_sceneScope != null)
                    m_saveProfile.SceneScope = m_sceneScope;
            }
        }

        internal SceneSerializationScope SceneScope {
            get => m_sceneScope;
            set => m_sceneScope = value ?? throw new ArgumentNullException(nameof(SceneScope));
        }

        private SaveProfile m_saveProfile;
        private SceneSerializationScope m_sceneScope;


        internal Game () {
            Name = "Game";
            Serializer = SaveSystem.Settings.SharedSerializer;
            KeyProvider = new KeyStore(SaveSystem.Settings.KeyMap);
            DataStorage = new FileSystemStorage(Storage.Root, SaveSystem.Settings.SharedSerializer.GetFormatCode());
        }


        public async Task Reload (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await OnReloadInvoke();
                if (SaveProfile != null)
                    await SaveProfile.Reload(token);
                else if (SceneScope != null)
                    await SceneScope.Reload(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(Name, "Data reload canceled");
            }
        }


        internal async Task Save (SaveType saveType, CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await OnSaveInvoke(saveType);
                if (SaveProfile != null)
                    await SaveProfile.Save(saveType, token);
                else if (SceneScope != null)
                    await SceneScope.Save(saveType, token);
            }
            catch (OperationCanceledException) {
                Logger.Log(Name, "Data saving canceled");
            }
        }

    }

}