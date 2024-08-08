using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Cryptography;
using SaveSystemPackage.Internal.Extensions;
using UnityEngine;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage.Security {

    public class Cryptographer : ScriptableObject, ICloneable<Cryptographer> {

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


        internal void SetSettings (EncryptionSettings settings) {
            passwordProvider = new DefaultKeyProvider(settings.password);
            saltProvider = new DefaultKeyProvider(settings.saltKey);
            generationParams = settings.keyGenerationParams;
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