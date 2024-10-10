using System;
using SaveSystemPackage.Internal.Security;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Settings {

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