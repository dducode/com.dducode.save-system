using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Cryptography;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Internal.Templates;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SaveSystemPackage.Security {

    public class VerificationManager {

        public HashAlgorithmName Algorithm { get; set; }


        public VerificationManager (VerificationSettings settings) {
            SetSettings(settings);
        }


        internal VerificationManager (HashAlgorithmName algorithm) {
            Algorithm = algorithm;
        }


        public void SetSettings (VerificationSettings settings) {
            Algorithm = settings.hashAlgorithm;
        }


        public void VerifyData ([NotNull] string filePath, [NotNull] byte[] data) {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            using DataTable table = DataTable.Open();
            byte[] storedHash = table[filePath];
            byte[] computedHash = ComputeHash(data, Algorithm);
            if (!storedHash.EqualsBytes(computedHash))
                throw new SecurityException(Messages.DataIsCorrupted);
        }


        public void SetChecksum ([NotNull] string filePath, [NotNull] byte[] data) {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            using DataTable table = DataTable.Open();
            table[filePath] = ComputeHash(data, Algorithm);
        }


        private byte[] ComputeHash (byte[] data, HashAlgorithmName algorithmName) {
            HashAlgorithm algorithm = algorithmName.SelectAlgorithm();
            return algorithm.ComputeHash(data);
        }

    }

}