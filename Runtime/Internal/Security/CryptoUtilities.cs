using System;
using System.Security.Cryptography;

namespace SaveSystemPackage.Internal.Security {

    internal static class CryptoUtilities {

        internal static string GenerateKey (int keyLength = 16) {
            var key = new byte[keyLength];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

    }

}