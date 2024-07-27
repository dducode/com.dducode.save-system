using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.Internal;

// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage {

    public class Game {

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

        /// <summary>
        /// Set the game data path
        /// </summary>
        [NotNull]
        public string DataPath {
            get => GameScope.DataPath;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataPath), "Data path cannot be null or empty");

                GameScope.DataPath = Storage.PrepareBeforeUsing(value);
            }
        }

        public SerializationSettings Settings => GameScope.Settings;
        public DataBuffer Data => GameScope.Data;

        internal SceneSerializationContext SceneContext {
            get => m_sceneContext;
            set {
                if (value == null)
                    throw new ArgumentNullException(nameof(SceneContext));
                m_sceneContext = value;
            }
        }

        internal bool HasChanges => Data.HasChanges;
        private SerializationScope GameScope { get; }

        private SaveProfile m_saveProfile;
        private SceneSerializationContext m_sceneContext;


        internal Game (SaveSystemSettings settings) {
            GameScope = new SerializationScope {
                Name = "Game scope",
                Settings = {
                    Encrypt = settings.encrypt,
                    VerifyChecksum = settings.verifyChecksum
                }
            };

            DataPath = settings.dataPath;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable"/>
        public Game RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            GameScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables"/>
        public Game RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            GameScope.RegisterSerializables(key, serializables);
            return this;
        }


        /// <summary>
        /// Start saving immediately and wait it
        /// </summary>
        public async UniTask Save (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await GameScope.Serialize(token);
                if (SaveProfile != null)
                    await SaveProfile.Save(token);
                else if (SceneContext != null)
                    await SceneContext.Save(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(GameScope.Name, "Data saving canceled");
            }
        }


        /// <summary>
        /// Start loading and wait it
        /// </summary>
        public async UniTask Load (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await GameScope.Deserialize(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(GameScope.Name, "Data loading canceled");
            }
        }


        internal async UniTask<StorageData> ExportGameData (CancellationToken token = default) {
            return File.Exists(DataPath)
                ? new StorageData(
                    await File.ReadAllBytesAsync(DataPath, token), Path.GetFileName(DataPath))
                : null;
        }


        internal async UniTask ImportGameData (byte[] data, CancellationToken token = default) {
            if (data.Length > 0)
                await File.WriteAllBytesAsync(DataPath, data, token);
        }

    }

}