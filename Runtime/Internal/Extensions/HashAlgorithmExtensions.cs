using System;
using System.Security.Cryptography;

namespace SaveSystem.Internal.Extensions {

    internal static class HashAlgorithmExtensions {

        internal static HashAlgorithmName SelectAlgorithmName (
            this SaveSystem.Cryptography.HashAlgorithmName algorithmName
        ) {
            switch (algorithmName) {
                case SaveSystem.Cryptography.HashAlgorithmName.SHA1:
                    return HashAlgorithmName.SHA1;
                case SaveSystem.Cryptography.HashAlgorithmName.SHA256:
                    return HashAlgorithmName.SHA256;
                case SaveSystem.Cryptography.HashAlgorithmName.SHA384:
                    return HashAlgorithmName.SHA384;
                case SaveSystem.Cryptography.HashAlgorithmName.SHA512:
                    return HashAlgorithmName.SHA512;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, null);
            }
        }


        internal static HashAlgorithm SelectAlgorithm (
            this SaveSystem.Cryptography.HashAlgorithmName algorithmName
        ) {
            switch (algorithmName) {
                case SaveSystem.Cryptography.HashAlgorithmName.SHA1:
                    return SHA1.Create();
                case SaveSystem.Cryptography.HashAlgorithmName.SHA256:
                    return SHA256.Create();
                case SaveSystem.Cryptography.HashAlgorithmName.SHA384:
                    return SHA384.Create();
                case SaveSystem.Cryptography.HashAlgorithmName.SHA512:
                    return SHA512.Create();
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, null);
            }
        }

    }

}