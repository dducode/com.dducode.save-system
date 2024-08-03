using System;
using SaveSystemPackage.Internal.Cryptography;

namespace SaveSystemPackage.Security {

    [Serializable]
    public class EncryptionSettings {

        public bool useCustomCryptographer;
        public string password = CryptoUtilities.GenerateKey();
        public string saltKey = CryptoUtilities.GenerateKey();
        public KeyGenerationParams keyGenerationParams = KeyGenerationParams.Default;


        public override string ToString () {
            string arg = useCustomCryptographer ? "enabled" : "disabled";
            return $"use custom cryptographer: {arg}, key generation parameters: {keyGenerationParams}";
        }

    }

}