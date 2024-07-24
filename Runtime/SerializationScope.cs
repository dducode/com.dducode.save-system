using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
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

        internal bool Authenticate {
            get => m_authenticate;
            set {
                m_authenticate = value;

                if (m_authenticate) {
                    using SaveSystemSettings settings = ResourcesManager.LoadSettings();

                    if (AuthManager == null)
                        AuthManager = new AuthenticationManager(settings.authenticationSettings);
                    else
                        AuthManager.SetSettings(settings.authenticationSettings);
                }
            }
        }

        [NotNull]
        internal AuthenticationManager AuthManager {
            get => m_authManager;
            set => m_authManager = value ?? throw new ArgumentNullException(nameof(AuthManager));
        }

        internal bool HasChanges { get; private set; }

        private string m_name;
        private string m_dataPath;

        private bool m_encrypt;
        private Cryptographer m_cryptographer;

        private bool m_authenticate;
        private AuthenticationManager m_authManager;

        private DataBuffer m_dataBuffer = new();
        private readonly Dictionary<string, IRuntimeSerializable> m_serializables = new();
        private int ObjectsCount => m_serializables.Count;
        private bool m_registrationClosed;


        internal void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_dataBuffer.Write(key, value);
            HasChanges = true;
        }


        internal void WriteData<TValue> ([NotNull] string key, TValue[] array) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_dataBuffer.Write(key, array);
            HasChanges = true;
        }


        internal void WriteData ([NotNull] string key, [NotNull] string value) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            m_dataBuffer.Write(key, value);
            HasChanges = true;
        }


        internal void WriteData ([NotNull] string key, MeshData meshData) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_dataBuffer.Write(key, meshData);
            HasChanges = true;
        }


        [Pure]
        internal TValue ReadData<TValue> ([NotNull] string key, TValue defaultValue = default)
            where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_dataBuffer.Count == 0 ? defaultValue : m_dataBuffer.Get(key, defaultValue);
        }


        [Pure]
        internal TValue[] ReadArray<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_dataBuffer.Count == 0 ? Array.Empty<TValue>() : m_dataBuffer.GetArray<TValue>(key);
        }


        [Pure]
        internal string ReadData ([NotNull] string key, string defaultValue = null) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_dataBuffer.Count == 0 ? defaultValue : m_dataBuffer.GetString(key, defaultValue);
        }


        [Pure]
        internal MeshData ReadMeshData ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_dataBuffer.Count == 0 ? default : m_dataBuffer.GetMeshData(key);
        }


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
            if (Authenticate && AuthManager == null)
                throw new InvalidOperationException("Authentication enabled but authentication manager doesn't set");

            if (ObjectsCount == 0 && m_dataBuffer.Count == 0)
                return;

            m_registrationClosed = true;

            byte[] data;

            using (var memoryStream = new MemoryStream()) {
                await using var writer = new SaveWriter(memoryStream);
                writer.Write(m_dataBuffer);
                SerializeObjects(writer);
                data = memoryStream.ToArray();
            }

            if (Encrypt)
                data = Cryptographer.Encrypt(data);
            if (Authenticate)
                AuthManager.SetAuthHash(DataPath, data);

            await File.WriteAllBytesAsync(DataPath, data, token);
            Logger.Log(Name, "Data saved");

            HasChanges = false;
        }


        internal async UniTask Deserialize (CancellationToken token) {
            if (Encrypt && Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer doesn't set");
            if (Authenticate && AuthManager == null)
                throw new InvalidOperationException("Authentication enabled but authentication manager doesn't set");

            m_registrationClosed = true;

            if (!File.Exists(DataPath)) {
                SetDefaults();
                return;
            }

            byte[] data = await File.ReadAllBytesAsync(DataPath, token);

            if (Authenticate)
                AuthManager.AuthenticateData(DataPath, data);
            if (Encrypt)
                data = Cryptographer.Decrypt(data);

            await using var reader = new SaveReader(new MemoryStream(data));

            m_dataBuffer = reader.ReadDataBuffer();
            DeserializeObjects(reader);
            Logger.Log(Name, "Data loaded");
        }


        private void SetDefaults () {
            m_registrationClosed = true;

            foreach (IDefault serializable in m_serializables.Select(pair => pair.Value as IDefault))
                serializable?.SetDefaults();
        }


        internal void Clear () {
            m_dataBuffer.Clear();
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