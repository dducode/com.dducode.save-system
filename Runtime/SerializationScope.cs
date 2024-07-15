using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.Internal.Templates;
using Logger = SaveSystem.Internal.Logger;

// ReSharper disable UnusedMember.Global

// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystem {

    public sealed class SerializationScope {

        [NotNull]
        public string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                m_name = value;
            }
        }

        private string m_name;

        private DataBuffer m_dataBuffer = new();
        private readonly Dictionary<string, IRuntimeSerializable> m_serializables = new();
        private int ObjectsCount => m_serializables.Count;
        private bool m_registrationClosed;


        public void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_dataBuffer.Write(key, value);
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (m_dataBuffer.Count == 0) {
                Logger.LogWarning(Name, "Data buffer is empty, return default value");
                return default;
            }

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_dataBuffer.Get<TValue>(key);
        }


        /// <summary>
        /// Registers an serializable object to save
        /// </summary>
        public void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
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
        public void RegisterSerializables (
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

            for (var i = 0; i < objects.Length; i++)
                m_serializables.Add($"{key}_{i}", objects[i]);

            DiagnosticService.AddObjects(objects);
            Logger.Log(Name, $"Serializable objects was registered in {Name}");
        }


        public byte[] SaveData () {
            if (ObjectsCount == 0 && m_dataBuffer.Count == 0)
                return null;

            m_registrationClosed = true;

            using var memoryStream = new MemoryStream();
            using var writer = new SaveWriter(memoryStream);

            writer.Write(m_dataBuffer);
            SerializeObjects(writer);

            Logger.Log(Name, "Data saved");
            return memoryStream.ToArray();
        }


        public void LoadData ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            m_registrationClosed = true;

            using var reader = new SaveReader(new MemoryStream(data));

            m_dataBuffer = reader.ReadDataBuffer();
            DeserializeObjects(reader);

            Logger.Log(Name, "Data loaded");
        }


        public void SetDefaults () {
            m_registrationClosed = true;

            foreach (IDefault serializable in m_serializables.Select(pair => pair.Value as IDefault))
                serializable?.SetDefaults();
        }


        private void SerializeObjects (SaveWriter writer) {
            foreach ((string key, IRuntimeSerializable serializable) in m_serializables) {
                writer.Write(key);
                serializable.Serialize(writer);
            }
        }


        private void DeserializeObjects (SaveReader reader) {
            foreach (KeyValuePair<string, IRuntimeSerializable> unused in m_serializables)
                m_serializables[reader.ReadString()].Deserialize(reader);
        }

    }

}