using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using SaveSystem.Internal.Extensions;
using SaveSystem.Internal.Templates;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SaveSystem.Security {

    public class AuthenticationManager {

        [NotNull]
        public string AuthHashKey {
            get => m_authHashKey;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(AuthHashKey));

                m_authHashKey = value;
            }
        }

        public HashAlgorithmName AlgorithmName { get; set; }
        private string m_authHashKey;


        public AuthenticationManager (AuthenticationSettings settings) {
            SetSettings(settings);
        }


        public AuthenticationManager (string authHashKey, HashAlgorithmName algorithmName) {
            AuthHashKey = authHashKey;
            AlgorithmName = algorithmName;
        }


        public void SetSettings (AuthenticationSettings settings) {
            AuthHashKey = settings.profileAuthHashKey;
            AlgorithmName = settings.hashAlgorithm;
        }


        public void AuthenticateData ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            string path = Path.Combine(
                SaveSystemCore.InternalFolder, $"{AuthHashKey}.{AlgorithmName.ToString().ToLower()}"
            );

            if (!File.Exists(path))
                throw new InvalidOperationException("There is no key for authenticate");

            byte[] storedHash = File.ReadAllBytes(path);
            byte[] computeHash = ComputeHash(data);
            if (storedHash.Length != computeHash.Length)
                throw new SecurityException(Messages.DataIsCorrupted);

            for (var i = 0; i < storedHash.Length; i++)
                if (storedHash[i] != computeHash[i])
                    throw new SecurityException(Messages.DataIsCorrupted);
        }


        public void SetAuthHash ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            string path = Path.Combine(
                SaveSystemCore.InternalFolder, $"{AuthHashKey}.{AlgorithmName.ToString().ToLower()}"
            );
            File.WriteAllBytes(path, ComputeHash(data));
        }


        private byte[] ComputeHash (byte[] data) {
            HashAlgorithm algorithm = AlgorithmName.SelectAlgorithm();
            return algorithm.ComputeHash(data);
        }

    }

}