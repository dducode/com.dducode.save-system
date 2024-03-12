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

        public bool Encrypt { get; set; }

        [NotNull]
        public Cryptographer Cryptographer {
            get => m_cryptographer;
            set => m_cryptographer = value ?? throw new ArgumentNullException(nameof(Cryptographer));
        }

        public bool Authenticate { get; set; }

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


        internal async UniTask<HandlingResult> SaveData ([NotNull] string dataPath, CancellationToken token) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            byte[] source = SaveData();
            if (source == null)
                return HandlingResult.Canceled;

            await File.WriteAllBytesAsync(dataPath, source, token).AsUniTask();
            return HandlingResult.Success;
        }


        internal async UniTask<HandlingResult> SaveData ([NotNull] Stream destination, CancellationToken token) {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            byte[] source = SaveData();
            if (source == null)
                return HandlingResult.Canceled;

            destination.Position = 0;
            await destination.WriteAsync(source, token).AsUniTask();
            return HandlingResult.Success;
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
                AuthManager.SetAuthHash(data);

            return data;
        }


        internal async UniTask<HandlingResult> LoadData ([NotNull] string dataPath, CancellationToken token) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            if (!File.Exists(dataPath)) {
                SerializationScope.SetDefaults();
                return HandlingResult.FileNotExists;
            }

            return LoadData(await File.ReadAllBytesAsync(dataPath, token).AsUniTask());
        }


        internal async UniTask<HandlingResult> LoadData ([NotNull] Stream stream, CancellationToken token = default) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var buffer = new byte[stream.Length];
            // ReSharper disable once MustUseReturnValue
            await stream.ReadAsync(buffer, token).AsUniTask();
            return LoadData(buffer);
        }


        internal HandlingResult LoadData ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            if (Authenticate)
                AuthManager.AuthenticateData(data);

            if (Encrypt)
                data = Cryptographer.Decrypt(data);

            return SerializationScope.LoadData(data);
        }

    }

}