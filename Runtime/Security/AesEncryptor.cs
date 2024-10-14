using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Internal.Security;
using SaveSystemPackage.Settings;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage.Security {

    public sealed class AesEncryptor : IEncryptor {

        private const string OperationWarning =
            "You {0} data that has length more than 85 000. You can use stream {1} instead";

        [NotNull]
        public ISecurityKeyProvider PasswordProvider {
            get => m_passwordProvider;
            set {
                m_passwordProvider = value ?? throw new ArgumentNullException(nameof(PasswordProvider));
                SaveSystem.Logger.Log(nameof(AesEncryptor), $"Set password provider: {value}");
            }
        }

        [NotNull]
        public ISecurityKeyProvider SaltProvider {
            get => m_saltProvider;
            set {
                m_saltProvider = value ?? throw new ArgumentNullException(nameof(SaltProvider));
                SaveSystem.Logger.Log(nameof(AesEncryptor), $"Set salt provider: {value}");
            }
        }

        public KeyGenerationParams GenerationParams {
            get => m_generationParams;
            set {
                m_generationParams = value;
                SaveSystem.Logger.Log(nameof(AesEncryptor), $"Set key generation params: {value}");
            }
        }

        private ISecurityKeyProvider m_passwordProvider;
        private ISecurityKeyProvider m_saltProvider;
        private KeyGenerationParams m_generationParams;


        public AesEncryptor (
            ISecurityKeyProvider passwordProvider, ISecurityKeyProvider saltProvider,
            KeyGenerationParams generationParams
        ) {
            m_passwordProvider = passwordProvider;
            m_saltProvider = saltProvider;
            m_generationParams = generationParams;
        }


        internal AesEncryptor (EncryptionSettings settings) {
            SetSettings(settings);
        }


        /// <summary>
        /// Encrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Data to be encrypted </param>
        /// <returns> Encrypted data </returns>
        public byte[] Encrypt ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length > 85_000) {
                SaveSystem.Logger.LogWarning(
                    nameof(AesEncryptor), string.Format(OperationWarning, "encrypt", "encryption")
                );
            }

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
        public byte[] Decrypt ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length > 85_000) {
                SaveSystem.Logger.LogWarning(
                    nameof(AesEncryptor), string.Format(OperationWarning, "decrypt", "decryption")
                );
            }

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


        internal void SetSettings (EncryptionSettings settings) {
            if (!settings.useCustomProviders) {
                m_passwordProvider = new SecurityKeyProvider(settings.password);
                m_saltProvider = new SecurityKeyProvider(settings.saltKey);
            }

            m_generationParams = settings.keyGenerationParams;
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