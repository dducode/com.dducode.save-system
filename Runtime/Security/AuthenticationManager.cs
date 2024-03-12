using System;
using System.Diagnostics.CodeAnalysis;
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

            if (!PlayerPrefs.HasKey(AuthHashKey))
                throw new InvalidOperationException("There is no key for authenticate");
            if (!string.Equals(PlayerPrefs.GetString(AuthHashKey), ComputeHash(data)))
                throw new SecurityException(Messages.DataIsCorrupted);
        }


        public void SetAuthHash ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            PlayerPrefs.SetString(AuthHashKey, ComputeHash(data));
            PlayerPrefs.Save();
        }


        private string ComputeHash (byte[] data) {
            HashAlgorithm algorithm = AlgorithmName.SelectAlgorithm();
            return Convert.ToBase64String(algorithm.ComputeHash(data));
        }

    }

}