using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Security;

// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage {

    public class Game {

        internal Game () { }

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

        public bool Encrypt {
            get => GameScope.Encrypt;
            set => GameScope.Encrypt = value;
        }

        /// <summary>
        /// Cryptographer used to encrypt/decrypt serializable data
        /// </summary>
        [NotNull]
        public Cryptographer Cryptographer {
            get => GameScope.Cryptographer;
            set => GameScope.Cryptographer = value;
        }

        public bool Authenticate {
            get => GameScope.Authenticate;
            set => GameScope.Authenticate = value;
        }

        [NotNull]
        public AuthenticationManager AuthManager {
            get => GameScope.AuthManager;
            set => GameScope.AuthManager = value;
        }

        internal SceneSerializationContext SceneContext {
            get => m_sceneContext;
            set {
                if (value == null)
                    throw new ArgumentNullException(nameof(SceneContext));
                m_sceneContext = value;
            }
        }

        internal bool HasChanges => GameScope.HasChanges;

        private SerializationScope GameScope { get; } = new() {
            Name = "Game scope"
        };

        private SaveProfile m_saveProfile;
        private SceneSerializationContext m_sceneContext;


        public Game WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            GameScope.WriteData(key, value);
            return this;
        }


        public Game WriteData<TValue> ([NotNull] string key, [NotNull] TValue[] array) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            GameScope.WriteData(key, array);
            return this;
        }


        public Game WriteData ([NotNull] string key, [NotNull] string value) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            GameScope.WriteData(key, value);
            return this;
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key, TValue defaultValue = default) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return GameScope.ReadData(key, defaultValue);
        }


        [Pure]
        public TValue[] ReadArray<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return GameScope.ReadArray<TValue>(key);
        }


        [Pure]
        public string ReadData ([NotNull] string key, string defaultValue = null) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return GameScope.ReadData(key, defaultValue);
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
                ? new StorageData(await File.ReadAllBytesAsync(DataPath, token), Path.GetFileName(DataPath))
                : null;
        }


        internal async UniTask ImportGameData (byte[] data, CancellationToken token = default) {
            if (data.Length > 0)
                await File.WriteAllBytesAsync(DataPath, data, token);
        }

    }

}