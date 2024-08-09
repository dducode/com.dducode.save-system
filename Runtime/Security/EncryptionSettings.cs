using System;
using SaveSystemPackage.Internal.Security;

namespace SaveSystemPackage.Security {

    [Serializable]
    public class EncryptionSettings {

        public bool useCustomCryptographer;
        public CryptographerReference reference;
        public bool useCustomProviders;
        public string password = CryptoUtilities.GenerateKey();
        public string saltKey = CryptoUtilities.GenerateKey();
        public KeyGenerationParams keyGenerationParams = KeyGenerationParams.Default;


        public override string ToString () {
            string arg = useCustomCryptographer ? "enabled" : "disabled";
            return $"use custom cryptographer: {arg}, key generation parameters: {keyGenerationParams}";
        }

    }

}