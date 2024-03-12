using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using SaveSystem.Internal.Extensions;
using SaveSystem.Internal.Templates;
using UnityEngine;

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
            #if UNITY_EDITOR
                Storage.AddPrefsKey(value);
            #endif
            }
        }

        public HashAlgorithmName AlgorithmName { get; set; }
        private string m_authHashKey;


        public AuthenticationManager (string authHashKey, HashAlgorithmName algorithmName) {
            AuthHashKey = authHashKey;
            AlgorithmName = algorithmName;
        }


        public void AuthenticateData ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            using var memoryStream = new MemoryStream(data);
            AuthenticateData(memoryStream);
        }


        public void AuthenticateData ([NotNull] Stream stream) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!PlayerPrefs.HasKey(AuthHashKey))
                throw new InvalidOperationException("There is no key for authenticate");
            if (!string.Equals(PlayerPrefs.GetString(AuthHashKey), ComputeHash(stream)))
                throw new SecurityException(Messages.DataIsCorrupted);
        }


        public void SetAuthHash ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            using var memoryStream = new MemoryStream(data);
            SetAuthHash(memoryStream);
        }


        public void SetAuthHash ([NotNull] Stream stream) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PlayerPrefs.SetString(AuthHashKey, ComputeHash(stream));
            PlayerPrefs.Save();
        }


        private string ComputeHash (Stream stream) {
            long oldPosition = stream.Position;
            HashAlgorithm algorithm = AlgorithmName.SelectAlgorithm();
            stream.Position = 0;
            byte[] hash = algorithm.ComputeHash(stream);
            stream.Position = oldPosition;
            return Convert.ToBase64String(hash);
        }

    }

}