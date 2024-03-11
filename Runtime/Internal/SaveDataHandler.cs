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

            await using FileStream fileStream = File.Open(dataPath, FileMode.OpenOrCreate);
            return await SaveData(fileStream, token);
        }


        internal async UniTask<HandlingResult> SaveData ([NotNull] Stream destination, CancellationToken token) {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            await using MemoryStream source = await SaveData(token);
            if (source == null)
                return HandlingResult.Canceled;

            destination.Position = 0;
            await destination.WriteAsync(source.ToArray(), token);
            return HandlingResult.Success;
        }


        [Pure]
        internal async UniTask<MemoryStream> SaveData (CancellationToken token) {
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer not sets");

            MemoryStream stream = await SerializationScope.SaveData(token);
            if (stream == null)
                return null;

            if (Encrypt)
                stream = await Cryptographer.Encrypt(stream, token);

            if (Authenticate)
                AuthManager.SetAuthHash(stream);

            return stream;
        }


        internal async UniTask<HandlingResult> LoadData ([NotNull] string dataPath, CancellationToken token) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            if (!File.Exists(dataPath)) {
                SerializationScope.SetDefaults();
                return HandlingResult.FileNotExists;
            }

            await using FileStream fileStream = File.Open(dataPath, FileMode.OpenOrCreate);
            return await LoadData(fileStream, token);
        }


        internal async UniTask<HandlingResult> LoadData (Stream source, CancellationToken token) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(source));
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Decryption enabled but cryptographer not sets");

            if (Authenticate)
                AuthManager.AuthenticateData(source);

            if (Encrypt)
                source = await Cryptographer.Decrypt(source, token);

            return await SerializationScope.LoadData(source, token);
        }

    }

}