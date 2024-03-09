﻿using System;
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

            (HandlingResult result, byte[] data) = await SaveData(token);
            if (result is HandlingResult.Success)
                await File.WriteAllBytesAsync(dataPath, data, token);
            return result;
        }


        [Pure]
        internal async UniTask<(HandlingResult, byte[])> SaveData (CancellationToken token) {
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer not sets");

            byte[] data = await SerializationScope.SaveData(token);
            if (data.Length > 0 && Encrypt)
                data = await Cryptographer.Encrypt(data, token);

            if (Authenticate)
                AuthManager.SetAuthHash(data);

            return (HandlingResult.Success, data);
        }


        internal async UniTask<HandlingResult> LoadData ([NotNull] string dataPath, CancellationToken token) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            if (!File.Exists(dataPath)) {
                SerializationScope.SetDefaults();
                return HandlingResult.FileNotExists;
            }

            return await LoadData(await File.ReadAllBytesAsync(dataPath, token), token);
        }


        internal async UniTask<HandlingResult> LoadData (byte[] data, CancellationToken token) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Decryption enabled but cryptographer not sets");

            if (Authenticate)
                AuthManager.AuthenticateData(data);

            if (Encrypt)
                data = await Cryptographer.Decrypt(data, token);

            return await SerializationScope.LoadData(data, token);
        }

    }

}