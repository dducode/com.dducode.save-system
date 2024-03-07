using UnityEngine;

namespace SaveSystem.Cryptography {

    [CreateAssetMenu(menuName = "Save System/Encryption Settings", fileName = nameof(EncryptionSettings))]
    public class EncryptionSettings : ScriptableObject {

        public bool useCustomProviders;
        public string password;
        public string saltKey;
        public KeyGenerationParams keyGenerationParams = KeyGenerationParams.Default;


        public override string ToString () {
            string arg = useCustomProviders ? "enabled" : "disabled";
            return $"use custom providers: {arg}, key generation parameters: {{{keyGenerationParams}}}";
        }

    }

}