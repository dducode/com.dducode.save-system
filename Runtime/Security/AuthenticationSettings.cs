using System;
using UnityEngine;

namespace SaveSystem.Security {

    [CreateAssetMenu(menuName = "Save System/Authentication Settings", fileName = nameof(AuthenticationSettings))]
    public class AuthenticationSettings : ScriptableObject {

        public HashAlgorithmName hashAlgorithm;
        public string globalAuthHashKey = Guid.NewGuid().ToString();
        public string profileAuthHashKey = Guid.NewGuid().ToString();


        public override string ToString () {
            return $"hash algorithm: {hashAlgorithm}";
        }

    }

}