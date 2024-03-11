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


        public virtual async UniTask<MemoryStream> Encrypt (
            [NotNull] Stream stream, CancellationToken token = default
        ) {
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
            stream.Position = 0;
            await stream.CopyToAsync(cryptoStream, token);
            cryptoStream.FlushFinalBlock();
            aes.Clear();
            await stream.DisposeAsync();

            return new MemoryStream(memoryStream.ToArray(), false);
        }


        public virtual async UniTask<MemoryStream> Decrypt (
            [NotNull] Stream stream, CancellationToken token = default
        ) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var aes = Aes.Create();
            byte[] key = GetKey(PasswordProvider.GetKey(), SaltProvider.GetKey(), GenerationParams);
            var iv = new byte[16];
            // ReSharper disable once MustUseReturnValue
            stream.Read(iv);

            await using var cryptoStream = new CryptoStream(
                stream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read
            );
            await using var memoryStream = new MemoryStream();
            await cryptoStream.CopyToAsync(memoryStream, token);
            aes.Clear();

            return new MemoryStream(memoryStream.ToArray(), false);
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