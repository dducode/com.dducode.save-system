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

namespace SaveSystem.Cryptography {

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


        public virtual async UniTask<byte[]> Encrypt ([NotNull] byte[] value, CancellationToken token = default) {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            byte[] iv = GetIV();

            using var memoryStream = new MemoryStream();
            memoryStream.Write(iv);

            var aes = Aes.Create();
            byte[] key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams);

            await using var cryptoStream = new CryptoStream(
                memoryStream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write
            );
            await cryptoStream.WriteAsync(value, token);
            cryptoStream.FlushFinalBlock();
            cryptoStream.Close();
            memoryStream.Close();

            return memoryStream.ToArray();
        }


        public virtual async UniTask<byte[]> Decrypt ([NotNull] byte[] value, CancellationToken token = default) {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var aes = Aes.Create();
            byte[] key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams);
            byte[] iv = value[..16];

            using var memoryStream = new MemoryStream(value[16..]);
            await using var cryptoStream = new CryptoStream(
                memoryStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read
            );

            var plainTextBytes = new byte[memoryStream.Length];
            int unused = await cryptoStream.ReadAsync(plainTextBytes, token);

            memoryStream.Close();
            cryptoStream.Close();

            return plainTextBytes;
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