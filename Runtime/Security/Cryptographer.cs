using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using SaveSystem.Internal.Cryptography;
using SaveSystem.Internal.Extensions;
using Logger = SaveSystem.Internal.Logger;

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
        /// <returns> Encrypted data </returns>
        public virtual byte[] Encrypt ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] iv = GetIV();

            using var memoryStream = new MemoryStream();
            memoryStream.Write(iv);

            using var aes = Aes.Create();
            byte[] key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams);

            using var cryptoStream = new CryptoStream(
                memoryStream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write
            );

            cryptoStream.Write(data);
            cryptoStream.FlushFinalBlock();
            aes.Clear();

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

            using var aes = Aes.Create();
            byte[] key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams);
            byte[] iv = data[..16];

            using var cryptoStream = new CryptoStream(
                new MemoryStream(data[16..]), aes.CreateDecryptor(key, iv), CryptoStreamMode.Read
            );

            var buffer = new byte[data.Length - 16];
            // ReSharper disable once MustUseReturnValue
            cryptoStream.Read(buffer);
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