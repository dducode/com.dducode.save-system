using System;

namespace SaveSystem.Security {

    [Serializable]
    public class AuthenticationSettings {

        public HashAlgorithmName hashAlgorithm;
        public string globalAuthHashKey = Guid.NewGuid().ToString();
        public string profileAuthHashKey = Guid.NewGuid().ToString();


        public override string ToString () {
            return $"hash algorithm: {hashAlgorithm}";
        }

    }

}