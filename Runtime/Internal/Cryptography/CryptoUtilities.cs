using System;
using System.Security.Cryptography;

namespace SaveSystem.Internal.Cryptography {

    internal class CryptoUtilities {

        internal static string GenerateKey () {
            var key = new byte[18];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

    }

}