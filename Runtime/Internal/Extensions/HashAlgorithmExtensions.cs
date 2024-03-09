using System;
using System.Security.Cryptography;

namespace SaveSystem.Internal.Extensions {

    internal static class HashAlgorithmExtensions {

        internal static HashAlgorithmName SelectAlgorithmName (
            this Security.HashAlgorithmName algorithmName
        ) {
            switch (algorithmName) {
                case Security.HashAlgorithmName.SHA1:
                    return HashAlgorithmName.SHA1;
                case Security.HashAlgorithmName.SHA256:
                    return HashAlgorithmName.SHA256;
                case Security.HashAlgorithmName.SHA384:
                    return HashAlgorithmName.SHA384;
                case Security.HashAlgorithmName.SHA512:
                    return HashAlgorithmName.SHA512;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, null);
            }
        }


        internal static HashAlgorithm SelectAlgorithm (
            this Security.HashAlgorithmName algorithmName
        ) {
            switch (algorithmName) {
                case Security.HashAlgorithmName.SHA1:
                    return SHA1.Create();
                case Security.HashAlgorithmName.SHA256:
                    return SHA256.Create();
                case Security.HashAlgorithmName.SHA384:
                    return SHA384.Create();
                case Security.HashAlgorithmName.SHA512:
                    return SHA512.Create();
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, null);
            }
        }

    }

}