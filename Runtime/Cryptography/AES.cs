using System.IO;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem.Cryptography {

    internal static class AES {

        internal static async UniTask<byte[]> Encrypt (
            byte[] value,
            IKeyProvider<string> passwordProvider,
            IKeyProvider<byte[]> saltProvider,
            AESKeyLength keyLength
        ) {
            string password = passwordProvider.GetKey();
            byte[] salt = saltProvider.GetKey();
            var symmetricKey = new RijndaelManaged {Mode = CipherMode.CBC, Padding = PaddingMode.Zeros};
            byte[] key = new Rfc2898DeriveBytes(password, salt).GetBytes((int)keyLength);
            byte[] iv = GetIV();
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(key, iv);

            using var memoryStream = new MemoryStream();
            memoryStream.Write(iv);

            await using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(value);
            cryptoStream.FlushFinalBlock();
            cryptoStream.Close();
            memoryStream.Close();

            return memoryStream.ToArray();
        }


        internal static async UniTask<byte[]> Decrypt (
            byte[] value,
            IKeyProvider<string> passwordProvider,
            IKeyProvider<byte[]> saltProvider,
            AESKeyLength keyLength
        ) {
            string password = passwordProvider.GetKey();
            byte[] salt = saltProvider.GetKey();
            var symmetricKey = new RijndaelManaged {Mode = CipherMode.CBC, Padding = PaddingMode.None};
            byte[] key = new Rfc2898DeriveBytes(password, salt).GetBytes((int)keyLength);
            byte[] iv = value[..16];
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(key, iv);

            using var memoryStream = new MemoryStream(value[16..]);
            await using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            var plainTextBytes = new byte[memoryStream.Length];
            int unused = await cryptoStream.ReadAsync(plainTextBytes);

            memoryStream.Close();
            cryptoStream.Close();

            return plainTextBytes;
        }


        private static byte[] GetIV () {
            var vi = new byte[16];
            RandomNumberGenerator.Create().GetBytes(vi);
            return vi;
        }

    }

}