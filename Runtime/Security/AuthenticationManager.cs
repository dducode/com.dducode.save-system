using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using SaveSystem.Internal.Extensions;
using SaveSystem.Internal.Templates;
using UnityEngine;

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


        public void AuthenticateData (Stream stream) {
            if (!PlayerPrefs.HasKey(AuthHashKey))
                throw new InvalidOperationException("There is no key for authenticate");
            if (!string.Equals(PlayerPrefs.GetString(AuthHashKey), ComputeHash(stream)))
                throw new SecurityException(Messages.DataIsCorrupted);
        }


        public void SetAuthHash (Stream stream) {
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