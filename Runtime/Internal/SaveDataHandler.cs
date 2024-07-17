using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Security;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem.Internal {

    internal class SaveDataHandler {

        public bool Encrypt {
            get => m_encrypt;
            set {
                m_encrypt = value;

                if (m_encrypt && Cryptographer == null) {
                    Cryptographer = new Cryptographer(
                        ResourcesManager.LoadSettings<SaveSystemSettings>().encryptionSettings
                    );
                }
            }
        }

        [NotNull]
        public Cryptographer Cryptographer {
            get => m_cryptographer;
            set => m_cryptographer = value ?? throw new ArgumentNullException(nameof(Cryptographer));
        }

        public bool Authenticate {
            get => m_authenticate;
            set {
                m_authenticate = value;

                if (m_authenticate && AuthManager == null) {
                    AuthManager = new AuthenticationManager(
                        ResourcesManager.LoadSettings<SaveSystemSettings>().authenticationSettings
                    );
                }
            }
        }

        [NotNull]
        public AuthenticationManager AuthManager {
            get => m_authManager;
            set => m_authManager = value ?? throw new ArgumentNullException(nameof(AuthManager));
        }

        [NotNull]
        public SerializationScope SerializationScope {
            get => m_serializationScope;
            set => m_serializationScope = value ?? throw new ArgumentNullException(nameof(SerializationScope));
        }

        private Cryptographer m_cryptographer;
        private AuthenticationManager m_authManager;
        private SerializationScope m_serializationScope;
        private bool m_encrypt;
        private bool m_authenticate;


        internal async UniTask SaveData ([NotNull] string dataPath, CancellationToken token = default) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            byte[] data = SaveData();
            if (data == null)
                return;

            await File.WriteAllBytesAsync(dataPath, data, token).AsUniTask();
        }


        [Pure]
        internal byte[] SaveData () {
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer not sets");

            byte[] data = SerializationScope.SaveData();
            if (data == null)
                return null;

            if (Encrypt)
                data = Cryptographer.Encrypt(data);

            if (Authenticate)
                data = AuthManager.SetAuthHash(data);

            return data;
        }


        internal async UniTask LoadData ([NotNull] string filePath, CancellationToken token = default) {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath)) {
                SerializationScope.SetDefaults();
                return;
            }

            LoadData(await File.ReadAllBytesAsync(filePath, token).AsUniTask());
        }


        internal void LoadData ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            if (Authenticate)
                data = AuthManager.AuthenticateData(data);

            if (Encrypt)
                data = Cryptographer.Decrypt(data);

            SerializationScope.LoadData(data);
        }

    }

}