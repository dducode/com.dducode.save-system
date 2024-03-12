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

            byte[] source = await SaveData(token);
            if (source == null)
                return HandlingResult.Canceled;

            destination.Position = 0;
            await destination.WriteAsync(source, token);
            return HandlingResult.Success;
        }


        [Pure]
        internal async UniTask<byte[]> SaveData (CancellationToken token) {
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer not sets");

            byte[] data = await SerializationScope.SaveData(token);
            if (data == null)
                return null;

            if (Encrypt)
                data = await Cryptographer.Encrypt(data, token);

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

            return await LoadData(File.Open(dataPath, FileMode.OpenOrCreate), token);
        }


        internal async UniTask<HandlingResult> LoadData ([NotNull] byte[] data, CancellationToken token = default) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            return await LoadData(new MemoryStream(data), token);
        }


        internal async UniTask<HandlingResult> LoadData (Stream source, CancellationToken token = default) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (Authenticate)
                AuthManager.AuthenticateData(source);

            if (Encrypt)
                source = new MemoryStream(await Cryptographer.Decrypt(source, token));

            return await SerializationScope.LoadData(source, token);
        }

    }

}