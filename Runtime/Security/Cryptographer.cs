using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal;
using SaveSystem.Internal.Cryptography;
using SaveSystem.Internal.Extensions;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem.Security {

    public class Cryptographer {

        [NotNull]
        public IKeyProvider PasswordProvider {
            get => m_passwordProvider;
            set {
                m_passwordProvider = value ?? throw new ArgumentNullException(nameof(PasswordProvider));
                Logger.Log(nameof(Cryptographer), $"Set password provider: {value}");
            }
        }

        [NotNull]
        public IKeyProvider SaltProvider {
            get => m_saltProvider;
            set {
                m_saltProvider = value ?? throw new ArgumentNullException(nameof(SaltProvider));
                Logger.Log(nameof(Cryptographer), $"Set salt provider: {value}");
            }
        }

        public KeyGenerationParams GenerationParams {
            get => m_generationParams;
            set {
                m_generationParams = value;
                Logger.Log(nameof(Cryptographer), $"Set key generation params: {value}");
            }
        }

        private IKeyProvider m_passwordProvider;
        private IKeyProvider m_saltProvider;
        private KeyGenerationParams m_generationParams;


        public Cryptographer (EncryptionSettings settings) {
            SetSettings(settings);
        }


        public Cryptographer (
            IKeyProvider passwordProvider, IKeyProvider saltProvider, KeyGenerationParams generationParams
        ) {
            m_passwordProvider = passwordProvider;
            m_saltProvider = saltProvider;
            m_generationParams = generationParams;
        }


        public void SetSettings (EncryptionSettings settings) {
            if (!settings.useCustomProviders) {
                m_passwordProvider = new DefaultKeyProvider(settings.password);
                m_saltProvider = new DefaultKeyProvider(settings.saltKey);
            }

            m_generationParams = settings.keyGenerationParams;
        }


        /// <summary>
        /// Encrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Data to be encrypted </param>
        /// <param name="token"></param>
        /// <returns> Ecnrypted data </returns>
        public virtual async UniTask<byte[]> Encrypt ([NotNull] byte[] data, CancellationToken token = default) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            await using var memoryStream = new MemoryStream(data);
            return await Encrypt(memoryStream, token);
        }


        /// <summary>
        /// Encrypts any data from a stream
        /// </summary>
        /// <param name="stream"> Stream to be encrypted </param>
        /// <param name="token"></param>
        /// <returns> Encrypted data </returns>
        public virtual async UniTask<byte[]> Encrypt ([NotNull] Stream stream, CancellationToken token = default) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            byte[] iv = GetIV();

            await using var memoryStream = new MemoryStream();
            memoryStream.Write(iv);

            using var aes = Aes.Create();
            byte[] key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams);

            await using var cryptoStream = new CryptoStream(
                memoryStream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write
            );

            await stream.CopyToAsync(cryptoStream, token);
            cryptoStream.FlushFinalBlock();
            aes.Clear();

            return memoryStream.ToArray();
        }


        /// <summary>
        /// Decrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Encrypted data </param>
        /// <param name="token"></param>
        /// <returns> Decrypted data </returns>
        public virtual async UniTask<byte[]> Decrypt ([NotNull] byte[] data, CancellationToken token = default) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            await using var memoryStream = new MemoryStream(data);
            return await Decrypt(memoryStream, token);
        }


        /// <summary>
        /// Decrypts any data from a stream
        /// </summary>
        /// <param name="stream"> Stream containing encrypted data </param>
        /// <param name="token"></param>
        /// <returns> Decrypted data </returns>
        public virtual async UniTask<byte[]> Decrypt ([NotNull] Stream stream, CancellationToken token = default) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var aes = Aes.Create();
            byte[] key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams);
            var iv = new byte[16];
            int readBytes = stream.Read(iv);

            await using var cryptoStream = new CryptoStream(
                stream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read, true
            );

            var buffer = new byte[stream.Length - readBytes];
            // ReSharper disable once MustUseReturnValue
            await cryptoStream.ReadAsync(buffer, token);
            aes.Clear();

            return buffer;
        }


        private byte[] GetKey (byte[] password, byte[] salt, KeyGenerationParams generationParams) {
            return new Rfc2898DeriveBytes(
                password, salt, generationParams.iterations, generationParams.hashAlgorithm.SelectAlgorithmName()
            ).GetBytes((int)generationParams.keyLength);
        }


        private byte[] GetIV () {
            var vi = new byte[16];
            RandomNumberGenerator.Fill(vi);
            return vi;
        }

    }

}