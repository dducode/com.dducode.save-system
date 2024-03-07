using System.Security.Cryptography;
using System.Text;

namespace SaveSystem.Internal.Cryptography {

    internal class CryptoUtilities {

        internal static string GenerateKey () {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!#$%&*";
            var result = new StringBuilder();

            for (var i = 0; i < 16; i++)
                result.Append(valid[RandomNumberGenerator.GetInt32(0, valid.Length)]);

            return result.ToString();
        }

    }

}