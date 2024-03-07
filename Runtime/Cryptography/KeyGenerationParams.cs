using System;
using UnityEngine;

namespace SaveSystem.Cryptography {

    [Serializable]
    public struct KeyGenerationParams {

        public static KeyGenerationParams Default => new(AESKeyLength._128Bit, HashAlgorithm.SHA1, 10);

        public AESKeyLength keyLength;
        public HashAlgorithm hashAlgorithm;

        [Min(1)]
        public int iterations;


        public KeyGenerationParams (AESKeyLength keyLength, HashAlgorithm hashAlgorithm, int iterations) {
            this.hashAlgorithm = hashAlgorithm;
            this.keyLength = keyLength;
            this.iterations = iterations;
        }


        public override string ToString () {
            return $"key length: {keyLength}, iterations: {iterations}, hash algorithm: {hashAlgorithm}";
        }

    }

}