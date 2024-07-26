using System;
using System.Security.Cryptography;

namespace SaveSystemPackage.Internal.Cryptography {

    internal static class CryptoUtilities {

        internal static string GenerateKey (int keyLength) {
            var key = new byte[keyLength];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }


        internal static string GenerateKey () {
            var key = new byte[18];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

    }

}