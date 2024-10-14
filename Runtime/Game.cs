using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Profiles;

// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage {

    public sealed class Game : SerializationContext {

        /// <summary>
        /// Uses for serializing data into a separately file
        /// </summary>
        [NotNull]
        public SaveProfile SaveProfile {
            get => m_saveProfile;
            set {
                m_saveProfile = value ?? throw new ArgumentNullException(nameof(SaveProfile));
                if (m_sceneContext != null)
                    m_saveProfile.SceneContext = m_sceneContext;
            }
        }

        internal SceneSerializationContext SceneContext {
            get => m_sceneContext;
            set => m_sceneContext = value ?? throw new ArgumentNullException(nameof(SceneContext));
        }

        private SaveProfile m_saveProfile;
        private SceneSerializationContext m_sceneContext;


        internal Game () {
            Name = "Game";
        }


        public async Task Reload (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await OnReloadInvoke();
                if (SaveProfile != null)
                    await SaveProfile.Reload(token);
                else if (SceneContext != null)
                    await SceneContext.Reload(token);
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.Log(Name, "Data reload canceled");
            }
        }


        internal async Task Save (SaveType saveType, CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await OnSaveInvoke(saveType);
                if (SaveProfile != null)
                    await SaveProfile.Save(saveType, token);
                else if (SceneContext != null)
                    await SceneContext.Save(saveType, token);
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.Log(Name, "Data saving canceled");
            }
        }

    }

}