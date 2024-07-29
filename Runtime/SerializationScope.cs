using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.Attributes;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal.Diagnostic;
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

        internal SerializationSettings Settings { get; } = new();
        internal DataBuffer Data { get; private set; } = new();
        private DataBuffer Buffer { get; set; } = new();

        private string m_name;
        private string m_dataPath;
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

            if (Buffer.Count > 0 && Buffer.ContainsKey(key)) {
                using var reader = new SaveReader(new MemoryStream(Buffer.ReadArray<byte>(key)));
                serializable.Deserialize(reader, reader.Read<int>());
                Buffer.Delete(key);
            }

            m_serializables.Add(key, serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(Name, $"Serializable object {serializable} registered in {Name}");
        }


        internal void RegisterSerializable ([NotNull] string key, [NotNull] object obj) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (!obj.GetType().IsDefined(typeof(RuntimeSerializableAttribute), false))
                throw new SerializationException($"The object {obj} must define RuntimeSerializable attribute");

            if (Buffer.Count > 0 && Buffer.ContainsKey(key)) {
                using var reader = new SaveReader(new MemoryStream(Buffer.ReadArray<byte>(key)));
                DeserializeGraph(reader, obj);
                Buffer.Delete(key);
            }

            m_objects.Add(key, obj);
            DiagnosticService.AddObject(obj);
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

            foreach ((string key, object graph) in m_objects) {
                using var stream = new MemoryStream();
                using var localWriter = new SaveWriter(stream);
                SerializeGraph(localWriter, graph);

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

            foreach (KeyValuePair<string, object> unused in m_objects) {
                using var localReader = new SaveReader(new MemoryStream(reader.ReadArray<byte>()));
                string key = Encoding.UTF8.GetString(reader.ReadArray<byte>());
                DeserializeGraph(localReader, m_objects[key]);
                --count;
            }

            for (var i = 0; i < count; i++) {
                byte[] bytes = reader.ReadArray<byte>();
                string key = Encoding.UTF8.GetString(reader.ReadArray<byte>());
                Buffer.Write(key, bytes);
            }
        }


        private void SerializeGraph (SaveWriter writer, object graph) {
            Type type = graph.GetType();
            FieldInfo[] fields = type
               .GetFields()
               .Concat(type
                   .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                   .Where(field => field.IsDefined(typeof(RuntimeSerializedFieldAttribute))))
               .ToArray();
            writer.Write(fields.Length);

            foreach (FieldInfo field in fields) {
                writer.Write(field.Name);
                writer.Write(field.FieldType.AssemblyQualifiedName);
                bool isPrimitive = field.FieldType.IsPrimitive;
                object value = field.GetValue(graph);
                writer.Write(isPrimitive);
                if (isPrimitive)
                    writer.Write(value);
                else
                    SerializeSubgraph(writer, value);
            }

            PropertyInfo[] properties = type
               .GetProperties()
               .Concat(type
                   .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                   .Where(property => property.IsDefined(typeof(RuntimeSerializedPropertyAttribute))))
               .ToArray();
            writer.Write(properties.Length);

            foreach (PropertyInfo property in properties) {
                writer.Write(property.Name);
                writer.Write(property.PropertyType.AssemblyQualifiedName);
                bool isPrimitive = property.PropertyType.IsPrimitive;
                object value = property.GetValue(graph);
                writer.Write(isPrimitive);
                if (isPrimitive)
                    writer.Write(value);
                else
                    SerializeSubgraph(writer, value);
            }
        }


        private void SerializeSubgraph (SaveWriter writer, object graph) {
            Type type = graph.GetType();

            if (graph is string str) {
                writer.Write(str);
                return;
            }

            if (type.IsArray) {
                var list = (Array)graph;
                writer.Write(list.Length);
                Type elementType = type.GetElementType() ?? throw new InvalidOperationException();
                writer.Write(elementType.AssemblyQualifiedName);

                foreach (object element in list) {
                    bool isPrimitive = elementType.IsPrimitive;
                    writer.Write(isPrimitive);
                    if (isPrimitive)
                        writer.Write(element);
                    else
                        SerializeSubgraph(writer, element);
                }

                return;
            }

            if (type.IsDefined(typeof(RuntimeSerializableAttribute)))
                SerializeGraph(writer, graph);
        }


        private void DeserializeGraph (SaveReader reader, object graph) {
            Type type = graph.GetType();
            var fieldsCount = reader.Read<int>();

            for (var i = 0; i < fieldsCount; i++) {
                string fieldName = reader.ReadString();
                var fieldType = Type.GetType(reader.ReadString());
                var isPrimitive = reader.Read<bool>();
                object value = isPrimitive ? reader.ReadObject(fieldType) : DeserializeSubgraph(reader, fieldType);

                if (value != default) {
                    FieldInfo field = type.GetField(
                        fieldName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    field?.SetValue(graph, value);
                }
            }

            var propertiesCount = reader.Read<int>();

            for (var i = 0; i < propertiesCount; i++) {
                string propertyName = reader.ReadString();
                var propertyType = Type.GetType(reader.ReadString());
                var isPrimitive = reader.Read<bool>();
                object value = isPrimitive
                    ? reader.ReadObject(propertyType)
                    : DeserializeSubgraph(reader, propertyType);

                if (value != default) {
                    PropertyInfo property = type.GetProperty(propertyName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    property?.SetValue(graph, value);
                }
            }
        }


        private object DeserializeSubgraph (SaveReader reader, Type type) {
            if (type == typeof(string))
                return reader.ReadString();

            if (type.IsArray) {
                var count = reader.Read<int>();
                var elementType = Type.GetType(reader.ReadString());
                var array = Array.CreateInstance(elementType ?? throw new InvalidOperationException(), count);

                for (var i = 0; i < count; i++) {
                    var isPrimitive = reader.Read<bool>();
                    array.SetValue(
                        isPrimitive ? reader.ReadObject(elementType) : DeserializeSubgraph(reader, elementType), i
                    );
                }

                return array;
            }

            if (type.IsDefined(typeof(RuntimeSerializableAttribute))) {
                object graph = Activator.CreateInstance(type);
                DeserializeGraph(reader, graph);
                return graph;
            }

            return default;
        }

    }

}