using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal.Diagnostic;
using SaveSystemPackage.Security;
using SaveSystemPackage.Verification;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    public sealed class SerializationScope {

        [NotNull]
        internal string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                m_name = value;
            }
        }

        [NotNull]
        internal string DataPath {
            get => m_dataPath;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(DataPath));

                m_dataPath = value;
            }
        }

        internal ScopeSettings Settings { get; } = new();
        internal DataBuffer Data { get; private set; } = new();
        private DataBuffer Buffer { get; set; } = new();

        private string m_name;
        private string m_dataPath;
        private readonly Dictionary<string, IRuntimeSerializable> m_serializables = new();
        private int ObjectsCount => m_serializables.Count;


        /// <summary>
        /// Registers an serializable object to save
        /// </summary>
        internal void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            if (Buffer.Count > 0 && Buffer.ContainsKey(key)) {
                using var reader = new SaveReader(new MemoryStream(Buffer.ReadArray<byte>(key)));
                serializable.Deserialize(reader, reader.Read<int>());
                Buffer.Delete(key);
            }

            m_serializables.Add(key, serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(Name, $"Serializable object {serializable} registered in {Name}");
        }


        /// <summary>
        /// Registers some serializable objects to save
        /// </summary>
        internal void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();

            if (objects.Length == 0)
                return;

            for (var i = 0; i < objects.Length; i++) {
                var singleKey = $"{key}_{i}";

                if (Buffer.Count > 0 && Buffer.ContainsKey(singleKey)) {
                    using var reader = new SaveReader(new MemoryStream(Buffer.ReadArray<byte>(singleKey)));
                    objects[i].Deserialize(reader, reader.Read<int>());
                    Buffer.Delete(singleKey);
                }

                m_serializables.Add(singleKey, objects[i]);
            }

            DiagnosticService.AddObjects(objects);
            Logger.Log(Name, $"Serializable objects was registered in {Name}");
        }


        internal async UniTask Serialize (CancellationToken token) {
            if (Settings.Encrypt && Settings.Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer doesn't set");
            if (Settings.VerifyChecksum && Settings.VerificationManager == null)
                throw new InvalidOperationException("Authentication enabled but authentication manager doesn't set");

            if (ObjectsCount == 0 && Data.Count == 0)
                return;

            byte[] data;

            using (var memoryStream = new MemoryStream()) {
                await using var writer = new SaveWriter(memoryStream);
                writer.Write(Data);
                SerializeObjects(writer);
                data = memoryStream.ToArray();
            }

            if (Settings.Encrypt)
                data = Settings.Cryptographer.Encrypt(data);
            if (Settings.VerifyChecksum)
                await Settings.VerificationManager.SetChecksum(DataPath, data);

            await File.WriteAllBytesAsync(DataPath, data, token);
            Logger.Log(Name, "Data saved");
        }


        internal async UniTask Deserialize (CancellationToken token) {
            if (Settings.Encrypt && Settings.Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer doesn't set");
            if (Settings.VerifyChecksum && Settings.VerificationManager == null)
                throw new InvalidOperationException("Authentication enabled but authentication manager doesn't set");

            if (!File.Exists(DataPath)) {
                SetDefaults();
                return;
            }

            byte[] data = await File.ReadAllBytesAsync(DataPath, token);

            if (Settings.VerifyChecksum)
                await Settings.VerificationManager.VerifyData(DataPath, data);
            if (Settings.Encrypt)
                data = Settings.Cryptographer.Decrypt(data);

            await using var reader = new SaveReader(new MemoryStream(data));

            Data = reader.ReadDataBuffer();
            DeserializeObjects(reader);
            Logger.Log(Name, "Data loaded");
        }


        internal void Clear () {
            Data.Clear();
            m_serializables.Clear();
        }


        private void SetDefaults () {
            foreach (IDefault serializable in m_serializables.Select(pair => pair.Value as IDefault))
                serializable?.SetDefaults();
        }


        private void SerializeObjects (SaveWriter writer) {
            writer.Write(ObjectsCount);

            foreach ((string key, IRuntimeSerializable serializable) in m_serializables) {
                using var stream = new MemoryStream();
                using var localWriter = new SaveWriter(stream);
                localWriter.Write(serializable.Version);
                serializable.Serialize(localWriter);

                writer.Write(stream.ToArray());
                writer.Write(Encoding.UTF8.GetBytes(key));
            }
        }


        private void DeserializeObjects (SaveReader reader) {
            var count = reader.Read<int>();

            foreach (KeyValuePair<string, IRuntimeSerializable> unused in m_serializables) {
                using var localReader = new SaveReader(new MemoryStream(reader.ReadArray<byte>()));
                string key = Encoding.UTF8.GetString(reader.ReadArray<byte>());
                m_serializables[key].Deserialize(localReader, localReader.Read<int>());
                --count;
            }

            for (var i = 0; i < count; i++) {
                byte[] bytes = reader.ReadArray<byte>();
                string key = Encoding.UTF8.GetString(reader.ReadArray<byte>());
                Buffer.Write(key, bytes);
            }
        }


        public sealed class ScopeSettings {

            internal bool Encrypt {
                get => m_encrypt;
                set {
                    m_encrypt = value;

                    if (m_encrypt) {
                        using SaveSystemSettings settings = SaveSystemSettings.Load();

                        if (Cryptographer == null)
                            Cryptographer = new Cryptographer(settings.encryptionSettings);
                        else
                            Cryptographer.SetSettings(settings.encryptionSettings);
                    }
                }
            }

            [NotNull]
            internal Cryptographer Cryptographer {
                get => m_cryptographer;
                set => m_cryptographer = value ?? throw new ArgumentNullException(nameof(Cryptographer));
            }

            internal bool VerifyChecksum {
                get => m_verifyChecksum;
                set {
                    m_verifyChecksum = value;

                    if (m_verifyChecksum) {
                        using SaveSystemSettings settings = SaveSystemSettings.Load();

                        if (VerificationManager == null)
                            VerificationManager = new VerificationManager(settings.verificationSettings);
                        else
                            VerificationManager.SetSettings(settings.verificationSettings);
                    }
                }
            }

            [NotNull]
            internal VerificationManager VerificationManager {
                get => m_verificationManager;
                set => m_verificationManager = value ?? throw new ArgumentNullException(nameof(VerificationManager));
            }

            private bool m_encrypt;
            private Cryptographer m_cryptographer;
            private bool m_verifyChecksum;
            private VerificationManager m_verificationManager;

        }

    }

}