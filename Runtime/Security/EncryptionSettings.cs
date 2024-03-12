using SaveSystem.Internal.Cryptography;
using UnityEngine;

namespace SaveSystem.Security {

    [CreateAssetMenu(menuName = "Save System/Encryption Settings", fileName = nameof(EncryptionSettings))]
    public class EncryptionSettings : ScriptableObject {

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