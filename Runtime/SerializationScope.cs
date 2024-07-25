using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Diagnostic;
using SaveSystemPackage.Internal.Templates;
using SaveSystemPackage.Security;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable UnusedMember.Global

// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    internal sealed class SerializationScope {

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

        internal bool Encrypt {
            get => m_encrypt;
            set {
                m_encrypt = value;

                if (m_encrypt) {
                    using SaveSystemSettings settings = ResourcesManager.LoadSettings();

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
                    using SaveSystemSettings settings = ResourcesManager.LoadSettings();

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

        internal DataBuffer DataBuffer { get; private set; } = new();

        private string m_name;
        private string m_dataPath;

        private bool m_encrypt;
        private Cryptographer m_cryptographer;

        private bool m_verifyChecksum;
        private VerificationManager m_verificationManager;

        private readonly Dictionary<string, IRuntimeSerializable> m_serializables = new();
        private int ObjectsCount => m_serializables.Count;
        private bool m_registrationClosed;


        /// <summary>
        /// Registers an serializable object to save
        /// </summary>
        internal void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

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
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();

            if (objects.Length == 0)
                return;

            for (var i = 0; i < objects.Length; i++)
                m_serializables.Add($"{key}_{i}", objects[i]);

            DiagnosticService.AddObjects(objects);
            Logger.Log(Name, $"Serializable objects was registered in {Name}");
        }


        internal async UniTask Serialize (CancellationToken token) {
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer doesn't set");
            if (VerifyChecksum && VerificationManager == null)
                throw new InvalidOperationException("Authentication enabled but authentication manager doesn't set");

            if (ObjectsCount == 0 && DataBuffer.Count == 0)
                return;

            m_registrationClosed = true;

            byte[] data;

            using (var memoryStream = new MemoryStream()) {
                await using var writer = new SaveWriter(memoryStream);
                writer.Write(DataBuffer);
                SerializeObjects(writer);
                data = memoryStream.ToArray();
            }

            if (Encrypt)
                data = Cryptographer.Encrypt(data);
            if (VerifyChecksum)
                VerificationManager.SetChecksum(DataPath, data);

            await File.WriteAllBytesAsync(DataPath, data, token);
            Logger.Log(Name, "Data saved");
        }


        internal async UniTask Deserialize (CancellationToken token) {
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer doesn't set");
            if (VerifyChecksum && VerificationManager == null)
                throw new InvalidOperationException("Authentication enabled but authentication manager doesn't set");

            m_registrationClosed = true;

            if (!File.Exists(DataPath)) {
                SetDefaults();
                return;
            }

            byte[] data = await File.ReadAllBytesAsync(DataPath, token);

            if (VerifyChecksum)
                VerificationManager.VerifyData(DataPath, data);
            if (Encrypt)
                data = Cryptographer.Decrypt(data);

            await using var reader = new SaveReader(new MemoryStream(data));

            DataBuffer = reader.ReadDataBuffer();
            DeserializeObjects(reader);
            Logger.Log(Name, "Data loaded");
        }


        private void SetDefaults () {
            m_registrationClosed = true;

            foreach (IDefault serializable in m_serializables.Select(pair => pair.Value as IDefault))
                serializable?.SetDefaults();
        }


        internal void Clear () {
            DataBuffer.Clear();
            m_serializables.Clear();
        }


        private void SerializeObjects (SaveWriter writer) {
            foreach ((string key, IRuntimeSerializable serializable) in m_serializables) {
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write(serializable.Version);
                serializable.Serialize(writer);
            }
        }


        private void DeserializeObjects (SaveReader reader) {
            foreach (KeyValuePair<string, IRuntimeSerializable> unused in m_serializables) {
                string key = Encoding.UTF8.GetString(reader.ReadArray<byte>());
                m_serializables[key].Deserialize(reader, reader.Read<int>());
            }
        }

    }

}