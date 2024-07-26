using System;
using SaveSystemPackage.Internal.Cryptography;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Verification {

    [Serializable]
    public class VerificationSettings {

        public HashAlgorithmName hashAlgorithm;
        public bool useCustomStorage;
        public string hashStoragePassword = CryptoUtilities.GenerateKey();
        public string hashStoragePath = "hash-storage.data";


        public override string ToString () {
            return $"hash algorithm: {hashAlgorithm}";
        }

    }

}