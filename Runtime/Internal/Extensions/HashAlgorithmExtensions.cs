using System;
using System.Security.Cryptography;
using HashAlgorithm = SaveSystem.Cryptography.HashAlgorithm;

namespace SaveSystem.Internal.Extensions {

    internal static class HashAlgorithmExtensions {

        internal static HashAlgorithmName SelectAlgorithmName (this HashAlgorithm hashAlgorithm) {
            switch (hashAlgorithm) {
                case HashAlgorithm.SHA1:
                    return HashAlgorithmName.SHA1;
                case HashAlgorithm.SHA256:
                    return HashAlgorithmName.SHA256;
                case HashAlgorithm.SHA384:
                    return HashAlgorithmName.SHA384;
                case HashAlgorithm.SHA512:
                    return HashAlgorithmName.SHA512;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), hashAlgorithm, null);
            }
        }

    }

}