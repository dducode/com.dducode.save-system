using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Attributes;
using SaveSystemPackage.Internal.Diagnostic;
using SaveSystemPackage.Security;
using SaveSystemPackage.Serialization;
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
        internal Internal.File DataFile {
            get => m_dataFile;
            set => m_dataFile = value ?? throw new ArgumentNullException(nameof(DataFile));
        }

        internal SerializationSettings Settings { get; } = new();
        internal DataBuffer Data { get; private set; } = new();
        internal SecureDataBuffer SecureData { get; private set; } = new();
        private DataBuffer Buffer { get; set; } = new();

        private string m_name;
        private Internal.File m_dataFile;
        private readonly Dictionary<string, IRuntimeSerializable> m_serializables = new();
        private readonly Dictionary<string, object> m_objects = new();
        private int ObjectsCount => m_serializables.Count + m_objects.Count;


        /// <summary>
        /// Registers an serializable object to save
        /// </summary>
        internal void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(key, serializable);
            DiagnosticService.AddObject(serializable);

            if (Buffer.Count > 0 && Buffer.ContainsKey(key)) {
                using var reader = new SaveReader(new MemoryStream(Buffer.ReadArray<byte>(key)));
                serializable.Deserialize(reader, reader.Read<int>());
                Buffer.Delete(key);
            }

            Logger.Log(Name, $"Serializable object {serializable} registered in {Name}");
        }


        internal void RegisterSerializable ([NotNull] string key, [NotNull] object obj) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (!obj.GetType().IsDefined(typeof(RuntimeSerializableAttribute), false))
                throw new SerializationException($"The object {obj} must define RuntimeSerializable attribute");

            m_objects.Add(key, obj);
            DiagnosticService.AddObject(obj);

            if (Buffer.Count > 0 && Buffer.ContainsKey(key)) {
                using var reader = new SaveReader(new MemoryStream(Buffer.ReadArray<byte>(key)));
                SerializationManager.DeserializeGraph(reader, obj);
                Buffer.Delete(key);
            }

            Logger.Log(Name, $"Serializable object {obj} registered in {Name}");
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


        internal async Task Serialize (CancellationToken token) {
            if (Settings.Encrypt && Settings.Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer doesn't set");

            if (ObjectsCount == 0 && Data.Count == 0 && SecureData.Count == 0)
                return;

            byte[] data;

            using (var memoryStream = new MemoryStream()) {
                await using var writer = new SaveWriter(memoryStream);
                writer.Write(Data);
                writer.Write(SecureData);
                SerializeObjects(writer);
                data = memoryStream.ToArray();
            }

            if (Settings.CompressFiles)
                data = Settings.FileCompressor.Compress(data);
            if (Settings.Encrypt)
                data = Settings.Cryptographer.Encrypt(data);

            await DataFile.WriteAllBytesAsync(data, token);
            Logger.Log(Name, "Data saved");
        }


        internal async Task Deserialize (CancellationToken token) {
            if (Settings.Encrypt && Settings.Cryptographer == null)
                throw new InvalidOperationException("Encryption enabled but cryptographer doesn't set");

            if (!DataFile.Exists) {
                SetDefaults();
                return;
            }

            byte[] data = await DataFile.ReadAllBytesAsync(token);

            if (Settings.Encrypt)
                data = Settings.Cryptographer.Decrypt(data);
            if (Settings.CompressFiles)
                data = Settings.FileCompressor.Decompress(data);

            await using var reader = new SaveReader(new MemoryStream(data));

            Data = reader.ReadDataBuffer();
            SecureData = reader.ReadSecureDataBuffer();
            DeserializeObjects(reader);
            Logger.Log(Name, "Data loaded");
        }


        internal void Clear () {
            Data.Clear();
            SecureData.Clear();
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
                byte[] bytes = Encoding.UTF8.GetBytes(key);
                writer.Write(bytes);
            }

            foreach ((string key, object graph) in m_objects) {
                using var stream = new MemoryStream();
                using var localWriter = new SaveWriter(stream);
                SerializationManager.SerializeGraph(localWriter, graph);

                writer.Write(stream.ToArray());
                writer.Write(Encoding.UTF8.GetBytes(key));
            }
        }


        private void DeserializeObjects (SaveReader reader) {
            var count = reader.Read<int>();

            foreach (KeyValuePair<string, IRuntimeSerializable> unused in m_serializables) {
                using var localReader = new SaveReader(new MemoryStream(reader.ReadArray<byte>()));
                byte[] bytes = reader.ReadArray<byte>();
                string key = Encoding.UTF8.GetString(bytes);
                if (m_serializables.TryGetValue(key, out IRuntimeSerializable serializable))
                    serializable.Deserialize(localReader, localReader.Read<int>());
                --count;
            }

            foreach (KeyValuePair<string, object> unused in m_objects) {
                using var localReader = new SaveReader(new MemoryStream(reader.ReadArray<byte>()));
                string key = Encoding.UTF8.GetString(reader.ReadArray<byte>());
                if (m_objects.TryGetValue(key, out object obj))
                    SerializationManager.DeserializeGraph(localReader, obj);
                --count;
            }

            for (var i = 0; i < count; i++) {
                byte[] bytes = reader.ReadArray<byte>();
                string key = Encoding.UTF8.GetString(reader.ReadArray<byte>());
                Buffer.Write(key, bytes);
            }
        }

    }

}