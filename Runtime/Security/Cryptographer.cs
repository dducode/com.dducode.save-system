using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Cryptography;
using SaveSystemPackage.Internal.Extensions;
using UnityEngine;
using File = SaveSystemPackage.Internal.File;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage.Security {

    public class Cryptographer : ScriptableObject, ICloneable<Cryptographer> {

        private const string OperationWarning =
            "You {0} data that has length more than 85 000. You can use stream {1} instead";

        [NotNull]
        public IKeyProvider PasswordProvider {
            get => passwordProvider;
            set {
                passwordProvider = value ?? throw new ArgumentNullException(nameof(PasswordProvider));
                Logger.Log(nameof(Cryptographer), $"Set password provider: {value}");
            }
        }

        [NotNull]
        public IKeyProvider SaltProvider {
            get => saltProvider;
            set {
                saltProvider = value ?? throw new ArgumentNullException(nameof(SaltProvider));
                Logger.Log(nameof(Cryptographer), $"Set salt provider: {value}");
            }
        }

        public KeyGenerationParams GenerationParams {
            get => generationParams;
            set {
                generationParams = value;
                Logger.Log(nameof(Cryptographer), $"Set key generation params: {value}");
            }
        }

        protected IKeyProvider passwordProvider;
        protected IKeyProvider saltProvider;
        protected KeyGenerationParams generationParams;


        public static TCryptographer CreateInstance<TCryptographer> (
            IKeyProvider passwordProvider, IKeyProvider saltProvider, KeyGenerationParams generationParams
        ) where TCryptographer : Cryptographer {
            var cryptographer = ScriptableObject.CreateInstance<TCryptographer>();
            cryptographer.passwordProvider = passwordProvider;
            cryptographer.saltProvider = saltProvider;
            cryptographer.generationParams = generationParams;
            return cryptographer;
        }


        internal static Cryptographer CreateInstance (EncryptionSettings settings) {
            var cryptographer = ScriptableObject.CreateInstance<Cryptographer>();
            cryptographer.SetSettings(settings);
            return cryptographer;
        }


        public virtual Cryptographer Clone () {
            return CreateInstance<Cryptographer>(passwordProvider.Clone(), saltProvider.Clone(), generationParams);
        }


        /// <summary>
        /// Encrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Data to be encrypted </param>
        /// <returns> Encrypted data </returns>
        public virtual byte[] Encrypt ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length > 85_000)
                Logger.LogWarning(nameof(Cryptographer), string.Format(OperationWarning, "encrypt", "encryption"));

            byte[] iv = GetIV();

            using var memoryStream = new MemoryStream();
            memoryStream.Write(iv);

            using var aes = Aes.Create();
            Key key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams).Pin();

            using var cryptoStream = new CryptoStream(
                memoryStream, aes.CreateEncryptor(key.value, iv), CryptoStreamMode.Write
            );

            cryptoStream.Write(data);
            cryptoStream.FlushFinalBlock();
            aes.Clear();
            key.Free();

            return memoryStream.ToArray();
        }


        /// <summary>
        /// Decrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Data containing encrypted data </param>
        /// <returns> Decrypted data </returns>
        public virtual byte[] Decrypt ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length > 85_000)
                Logger.LogWarning(nameof(Cryptographer), string.Format(OperationWarning, "decrypt", "decryption"));

            using var aes = Aes.Create();
            Key key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams).Pin();
            byte[] iv = data[..16];

            using var cryptoStream = new CryptoStream(
                new MemoryStream(data[16..]), aes.CreateDecryptor(key.value, iv), CryptoStreamMode.Read
            );

            var buffer = new byte[data.Length - 16];
            // ReSharper disable once MustUseReturnValue
            cryptoStream.Read(buffer);
            aes.Clear();
            key.Free();

            return buffer;
        }


        /// <summary>
        /// Encrypts any data from a byte array
        /// </summary>
        /// <param name="stream"> Stream to be encrypted </param>
        /// <param name="token"></param>
        public virtual async Task Encrypt ([NotNull] Stream stream, CancellationToken token = default) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            stream.Position = 0;
            File cacheFile = Storage.CacheRoot.CreateFile("encrypt", "temp");

            try {
                await using FileStream cacheStream = cacheFile.Open();

                byte[] iv = GetIV();
                cacheStream.Write(iv);

                using var aes = Aes.Create();
                Key key = await Task.Run(() =>
                    GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams).Pin(), token);

                await using var cryptoStream = new CryptoStream(
                    cacheStream, aes.CreateEncryptor(key.value, iv), CryptoStreamMode.Write
                );

                await stream.CopyToAsync(cryptoStream, token);
                cryptoStream.FlushFinalBlock();
                aes.Clear();
                key.Free();

                stream.SetLength(0);
                cacheStream.Position = 0;
                await cacheStream.CopyToAsync(stream, token);
            }
            finally {
                cacheFile.Delete();
                stream.Position = 0;
            }
        }


        /// <summary>
        /// Decrypts any data from a byte array
        /// </summary>
        /// <param name="stream"> Stream containing encrypted data </param>
        /// <param name="token"></param>
        public virtual async Task Decrypt ([NotNull] Stream stream, CancellationToken token = default) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            stream.Position = 0;
            File cacheFile = Storage.CacheRoot.CreateFile("decrypt", "temp");

            try {
                await using FileStream cacheStream = cacheFile.Open();

                await stream.CopyToAsync(cacheStream, token);
                stream.SetLength(0);
                cacheStream.Position = 0;
                var iv = new byte[16];
                // ReSharper disable once MustUseReturnValue
                int readBytes = cacheStream.Read(iv);
                cacheStream.Position = readBytes;

                using var aes = Aes.Create();
                Key key = await Task.Run(() =>
                    GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams).Pin(), token);
                await using var cryptoStream = new CryptoStream(
                    cacheStream, aes.CreateDecryptor(key.value, iv), CryptoStreamMode.Read
                );

                await cryptoStream.CopyToAsync(stream, token);
                aes.Clear();
                key.Free();
            }
            finally {
                cacheFile.Delete();
                stream.Position = 0;
            }
        }


        internal void SetSettings (EncryptionSettings settings) {
            passwordProvider = new DefaultKeyProvider(settings.password);
            saltProvider = new DefaultKeyProvider(settings.saltKey);
            generationParams = settings.keyGenerationParams;
        }


        private Key GetKey (Key password, Key salt, KeyGenerationParams generationParams) {
            password.Pin();
            salt.Pin();
            var key = new Key(
                new Rfc2898DeriveBytes(
                    password.value, salt.value, generationParams.iterations,
                    generationParams.hashAlgorithm.SelectAlgorithmName()
                ).GetBytes((int)generationParams.keyLength)
            );
            password.Free();
            salt.Free();
            return key;
        }


        private byte[] GetIV () {
            var vi = new byte[16];
            RandomNumberGenerator.Fill(vi);
            return vi;
        }

    }

}