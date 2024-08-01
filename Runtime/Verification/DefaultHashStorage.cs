using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.Security;
using SaveSystemPackage.Serialization;
using File = SaveSystemPackage.Internal.File;
using HashAlgorithmName = System.Security.Cryptography.HashAlgorithmName;

namespace SaveSystemPackage.Verification {

    public class DefaultHashStorage : HashStorage {

        private const int IVLength = 16;
        private File StorageFile => SaveSystem.HashStorageFile;

        public override byte[] this [string key] {
            get {
                if (!map.ContainsKey(key))
                    throw new ArgumentException($"Data table doesn't contain given key: {key}");
                return map[key];
            }
            set {
                if (!map.TryAdd(key, value))
                    map[key] = value;
            }
        }


        public override void Add (File file, byte[] bytes) {
            string name = file.FullName;

            if (file.Directory.TryGetFile(name, out File link)) {
                map[link.ReadAllText()] = bytes;
            }
            else {
                link = file.Directory.CreateFile(name, "link");
                var guid = Guid.NewGuid().ToString();
                link.WriteAllText(guid);
                map[guid] = bytes;
            }
        }


        public override byte[] Get (File file) {
            File link = file.Directory.GetFile(file.FullName);
            return map[link.ReadAllText()];
        }


        public override void RenameLink (File file) {
            if (file.Directory.TryGetFile(file.OldFullName, out File link))
                link.Rename(file.FullName);
        }


        public override UniTask Open () {
            if (!StorageFile.Exists)
                return UniTask.CompletedTask;

            byte[] data = StorageFile.ReadAllBytes();
            byte[] buffer;

            string password;

            using (SaveSystemSettings settings = SaveSystemSettings.Load()) {
                password = settings.verificationSettings.hashStoragePassword;
            }

            using var aes = Aes.Create();
            byte[] key = GetKey(password);
            byte[] iv = data[..IVLength];

            using (var cryptoStream = new CryptoStream(
                new MemoryStream(data[IVLength..]), aes.CreateDecryptor(key, iv), CryptoStreamMode.Read)
            ) {
                buffer = new byte[data.Length - IVLength];
                // ReSharper disable once MustUseReturnValue
                cryptoStream.Read(buffer);
                aes.Clear();
            }

            using (var reader = new SaveReader(new MemoryStream(buffer))) {
                var rows = reader.Read<int>();
                for (var i = 0; i < rows; i++)
                    map.Add(Encoding.UTF8.GetString(reader.ReadArray<byte>()), reader.ReadArray<byte>());
            }

            return UniTask.CompletedTask;
        }


        public override UniTask Close () {
            byte[] buffer;

            using (var memoryStream = new MemoryStream()) {
                using var writer = new SaveWriter(memoryStream);

                writer.Write(map.Count);

                foreach ((string key, byte[] bytes) in map) {
                    writer.Write(Encoding.UTF8.GetBytes(key));
                    writer.Write(bytes);
                }

                buffer = memoryStream.ToArray();
            }

            map.Clear();

            string password;

            using (SaveSystemSettings settings = SaveSystemSettings.Load()) {
                password = settings.verificationSettings.hashStoragePassword;
            }

            using (var memoryStream = new MemoryStream()) {
                byte[] iv = GetIV();
                memoryStream.Write(iv);
                using var aes = Aes.Create();
                byte[] key = GetKey(password);

                using var cryptoStream = new CryptoStream(
                    memoryStream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write
                );

                cryptoStream.Write(buffer);
                cryptoStream.FlushFinalBlock();
                aes.Clear();

                StorageFile.WriteAllBytes(memoryStream.ToArray());
            }

            return UniTask.CompletedTask;
        }


        private byte[] GetIV () {
            var vi = new byte[IVLength];
            RandomNumberGenerator.Fill(vi);
            return vi;
        }


        private byte[] GetKey (string password) {
            return new Rfc2898DeriveBytes(
                password, SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password)), 10, HashAlgorithmName.SHA1
            ).GetBytes((int)AESKeyLength._128Bit);
        }

    }

}