using System;
using SaveSystemPackage.Internal.Cryptography;

namespace SaveSystemPackage.Security {

    [Serializable]
    public class EncryptionSettings {

        public bool useCustomProviders;
        public string password = CryptoUtilities.GenerateKey();
        public string saltKey = CryptoUtilities.GenerateKey();
        public KeyGenerationParams keyGenerationParams = KeyGenerationParams.Default;


        public override string ToString () {
            string arg = useCustomProviders ? "enabled" : "disabled";
            return $"use custom providers: {arg}, key generation parameters: {{{keyGenerationParams}}}";
        }

    }

}