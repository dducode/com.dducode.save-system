using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.CloudSave;
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
                m_globalScope.AttachNestedScope(m_saveProfile.ProfileScope);
                if (m_sceneContext != null)
                    m_saveProfile.SceneContext = m_sceneContext;
            }
        }

        /// <summary>
        /// Set the global data path
        /// </summary>
        [NotNull]
        public string DataPath {
            get => m_globalScope.DataPath;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataPath), "Data path cannot be null or empty");

                m_globalScope.DataPath = Storage.PrepareBeforeUsing(value);
            }
        }

        public bool Encrypt {
            get => m_globalScope.Encrypt;
            set => m_globalScope.Encrypt = value;
        }

        /// <summary>
        /// Cryptographer used to encrypt/decrypt serializable data
        /// </summary>
        [NotNull]
        public Cryptographer Cryptographer {
            get => m_globalScope.Cryptographer;
            set => m_globalScope.Cryptographer = value;
        }

        public bool Authenticate {
            get => m_globalScope.Authenticate;
            set => m_globalScope.Authenticate = value;
        }

        [NotNull]
        public AuthenticationManager AuthManager {
            get => m_globalScope.AuthManager;
            set => m_globalScope.AuthManager = value;
        }

        internal SceneSerializationContext SceneContext {
            get => m_sceneContext;
            set {
                if (value == null)
                    throw new ArgumentNullException(nameof(SceneContext));
                m_sceneContext = value;
                m_globalScope.AttachNestedScope(m_sceneContext.SceneScope);
            }
        }

        private readonly SerializationScope m_globalScope = new() {
            Name = "Global scope"
        };

        private SaveProfile m_saveProfile;
        private SceneSerializationContext m_sceneContext;


        public void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_globalScope.WriteData(key, value);
        }


        public void WriteData<TValue> ([NotNull] string key, [NotNull] TValue[] array) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            m_globalScope.WriteData(key, array);
        }


        public void WriteData ([NotNull] string key, [NotNull] string value) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            m_globalScope.WriteData(key, value);
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key, TValue defaultValue = default) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_globalScope.ReadData(key, defaultValue);
        }


        [Pure]
        public TValue[] ReadArray<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_globalScope.ReadArray<TValue>(key);
        }


        [Pure]
        public string ReadData ([NotNull] string key, string defaultValue = null) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_globalScope.ReadData(key, defaultValue);
        }


        public async UniTask Save (CancellationToken token = default) {
            await SaveSystem.SaveScope(m_globalScope, token);
        }


        public async UniTask Load (CancellationToken token = default) {
            await SaveSystem.LoadScope(m_globalScope, token);
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