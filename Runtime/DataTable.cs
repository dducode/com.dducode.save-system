using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal;
using SaveSystem.Security;

namespace SaveSystem {

    public class DataTable : IDisposable {

        private const int IVLength = 16;
        internal static string Path => System.IO.Path.Combine(SaveSystemCore.InternalFolder, "datatable.data");

        public byte[] this [string key] {
            get {
                if (!m_map.ContainsKey(key))
                    throw new ArgumentException($"Data table doesn't contain given key: {key}");
                return m_map[key];
            }
            set {
                if (!m_map.TryAdd(key, value))
                    m_map[key] = value;
            }
        }

        private readonly Dictionary<string, byte[]> m_map = new();


        public static DataTable Open () {
            if (!File.Exists(Path))
                return new DataTable();

            byte[] data = File.ReadAllBytes(Path);
            byte[] buffer;

            SaveSystemSettings settings = ResourcesManager.LoadSettings();
            string password = settings.authenticationSettings.dataTablePassword;
            ResourcesManager.UnloadSettings(settings);

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

            var table = new DataTable();

            using (var reader = new SaveReader(new MemoryStream(buffer))) {
                var rows = reader.Read<int>();
                for (var i = 0; i < rows; i++)
                    table.Add(reader.ReadString(), reader.ReadArray<byte>());
            }

            return table;
        }


        private static byte[] GetIV () {
            var vi = new byte[IVLength];
            RandomNumberGenerator.Fill(vi);
            return vi;
        }


        private static byte[] GetKey (string password) {
            return new Rfc2898DeriveBytes(
                password, SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password))
            ).GetBytes((int)AESKeyLength._128Bit);
        }


        public void Close () {
            byte[] buffer;

            using (var memoryStream = new MemoryStream()) {
                using var writer = new SaveWriter(memoryStream);

                writer.Write(m_map.Count);

                foreach ((string key, byte[] bytes) in m_map) {
                    writer.Write(key);
                    writer.Write(bytes);
                }

                buffer = memoryStream.ToArray();
            }

            SaveSystemSettings settings = ResourcesManager.LoadSettings();
            string password = settings.authenticationSettings.dataTablePassword;
            ResourcesManager.UnloadSettings(settings);

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

                File.WriteAllBytes(Path, memoryStream.ToArray());
            }
        }


        public void Dispose () {
            Close();
        }


        private void Add (string key, byte[] hash) {
            m_map.Add(key, hash);
        }

    }

}