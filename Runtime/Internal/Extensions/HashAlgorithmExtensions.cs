using System;
using System.Security.Cryptography;

namespace SaveSystemPackage.Internal.Extensions {

    internal static class HashAlgorithmExtensions {

        internal static HashAlgorithmName SelectAlgorithmName (
            this SaveSystemPackage.Security.HashAlgorithmName algorithmName
        ) {
            switch (algorithmName) {
                case SaveSystemPackage.Security.HashAlgorithmName.SHA1:
                    return HashAlgorithmName.SHA1;
                case SaveSystemPackage.Security.HashAlgorithmName.SHA256:
                    return HashAlgorithmName.SHA256;
                case SaveSystemPackage.Security.HashAlgorithmName.SHA384:
                    return HashAlgorithmName.SHA384;
                case SaveSystemPackage.Security.HashAlgorithmName.SHA512:
                    return HashAlgorithmName.SHA512;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, null);
            }
        }

    }

}