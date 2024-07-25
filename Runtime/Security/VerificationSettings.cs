using System;
using SaveSystemPackage.Internal.Cryptography;

namespace SaveSystemPackage.Security {

    [Serializable]
    public class VerificationSettings {

        public HashAlgorithmName hashAlgorithm;
        public string dataTablePassword = CryptoUtilities.GenerateKey();


        public override string ToString () {
            return $"hash algorithm: {hashAlgorithm}";
        }

    }

}