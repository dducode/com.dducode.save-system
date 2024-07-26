using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal;

// ReSharper disable UnusedMember.Global
namespace SaveSystemPackage {

    /// <summary>
    /// The buffer for writing and reading data
    /// </summary>
    public record DataBuffer {

        private readonly Dictionary<string, byte[]> m_commonBuffer;
        private readonly Dictionary<string, KeyValuePair<int, byte[]>> m_arrayBuffer;
        private readonly Dictionary<string, string> m_stringBuffer;
        private readonly Dictionary<string, MeshData> m_meshDataBuffer;
        private readonly byte[] m_encodingKey = GenerateKey();

        public int Count => m_commonBuffer.Count + m_arrayBuffer.Count + m_stringBuffer.Count + m_meshDataBuffer.Count;
        public bool HasChanges { get; private set; }


        public DataBuffer () {
            m_commonBuffer = new Dictionary<string, byte[]>();
            m_arrayBuffer = new Dictionary<string, KeyValuePair<int, byte[]>>();
            m_stringBuffer = new Dictionary<string, string>();
            m_meshDataBuffer = new Dictionary<string, MeshData>();
        }


        internal DataBuffer (SaveReader reader) {
            m_commonBuffer = ReadCommonBuffer(reader);
            m_arrayBuffer = ReadArrayBuffer(reader);
            m_stringBuffer = ReadStringBuffer(reader);
            m_meshDataBuffer = ReadMeshDataBuffer(reader);
        }


        private static byte[] GenerateKey () {
            var key = new byte[16];
            RandomNumberGenerator.Fill(key);
            return key;
        }


        public void Write<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            bool containsKey = m_arrayBuffer.ContainsKey(key) || m_stringBuffer.ContainsKey(key) ||
                               m_meshDataBuffer.ContainsKey(key);

            if (containsKey) {
                Logger.LogError(nameof(DataBuffer), $"The buffer already contains data with given key: {key}");
                return;
            }

            m_commonBuffer[key] = Encode(
                MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToArray()
            );
            HasChanges = true;
        }


        [Pure]
        public TValue Read<TValue> ([NotNull] string key, TValue defaultValue = default) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_commonBuffer.TryGetValue(key, out byte[] value)
                ? MemoryMarshal.Read<TValue>(Encode(value))
                : defaultValue;
        }


        public void Write<TArray> ([NotNull] string key, [NotNull] TArray[] array) where TArray : unmanaged {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            bool containsKey = m_commonBuffer.ContainsKey(key) || m_stringBuffer.ContainsKey(key) ||
                               m_meshDataBuffer.ContainsKey(key);

            if (containsKey) {
                Logger.LogError(nameof(DataBuffer), $"The buffer already contains data with given key: {key}");
                return;
            }

            m_arrayBuffer[key] = new KeyValuePair<int, byte[]>(
                array.Length,
                Encode(MemoryMarshal.AsBytes((ReadOnlySpan<TArray>)array).ToArray())
            );
            HasChanges = true;
        }


        [Pure]
        public TArray[] ReadArray<TArray> ([NotNull] string key) where TArray : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (m_arrayBuffer.ContainsKey(key)) {
                var array = new TArray[m_arrayBuffer[key].Key];
                Span<byte> span = MemoryMarshal.AsBytes((Span<TArray>)array);
                byte[] data = Encode(m_arrayBuffer[key].Value);
                for (var i = 0; i < span.Length; i++)
                    span[i] = data[i];
                return array;
            }
            else {
                return Array.Empty<TArray>();
            }
        }


        public void Write ([NotNull] string key, [NotNull] string value) {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            bool containsKey = m_commonBuffer.ContainsKey(key) || m_arrayBuffer.ContainsKey(key) ||
                               m_meshDataBuffer.ContainsKey(key);

            if (containsKey) {
                Logger.LogError(nameof(DataBuffer), $"The buffer already contains data with given key: {key}");
                return;
            }

            m_stringBuffer[key] = Convert.ToBase64String(Encode(Encoding.Default.GetBytes(value)));
            HasChanges = true;
        }


        [Pure]
        public string ReadString ([NotNull] string key, string defaultValue = null) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return Encoding.Default.GetString(
                Encode(Convert.FromBase64String(m_stringBuffer.GetValueOrDefault(key, defaultValue)))
            );
        }


        public void Write ([NotNull] string key, MeshData value) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            bool containsKey = m_commonBuffer.ContainsKey(key) || m_arrayBuffer.ContainsKey(key) ||
                               m_stringBuffer.ContainsKey(key);

            if (containsKey) {
                Logger.LogError(nameof(DataBuffer), $"The buffer already contains data with given key: {key}");
                return;
            }

            m_meshDataBuffer[key] = value;
            HasChanges = true;
        }


        [Pure]
        public MeshData ReadMeshData ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_meshDataBuffer.GetValueOrDefault(key);
        }


        public bool Delete ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_commonBuffer.Remove(key) ||
                   m_arrayBuffer.Remove(key) ||
                   m_stringBuffer.Remove(key) ||
                   m_meshDataBuffer.Remove(key);
        }


        public bool ContainsKey ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_commonBuffer.ContainsKey(key) ||
                   m_arrayBuffer.ContainsKey(key) ||
                   m_stringBuffer.ContainsKey(key) ||
                   m_meshDataBuffer.ContainsKey(key);
        }


        internal void WriteData (SaveWriter writer) {
            WriteBuffer(m_commonBuffer, writer);
            WriteBuffer(m_arrayBuffer, writer);
            WriteBuffer(m_stringBuffer, writer);
            WriteBuffer(m_meshDataBuffer, writer);
            HasChanges = false;
        }


        internal void Clear () {
            m_commonBuffer.Clear();
            m_arrayBuffer.Clear();
            m_stringBuffer.Clear();
            m_meshDataBuffer.Clear();
        }


        private void WriteBuffer (Dictionary<string, byte[]> buffer, SaveWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write(Encode(buffer[key]));
            }
        }


        private Dictionary<string, byte[]> ReadCommonBuffer (SaveReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, byte[]>();

            for (var i = 0; i < count; i++)
                buffer.Add(Encoding.UTF8.GetString(reader.ReadArray<byte>()), Encode(reader.ReadArray<byte>()));

            return buffer;
        }


        private void WriteBuffer (Dictionary<string, KeyValuePair<int, byte[]>> buffer, SaveWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write(buffer[key].Key);
                writer.Write(Encode(buffer[key].Value));
            }
        }


        private Dictionary<string, KeyValuePair<int, byte[]>> ReadArrayBuffer (SaveReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, KeyValuePair<int, byte[]>>();

            for (var i = 0; i < count; i++) {
                string key = Encoding.UTF8.GetString(reader.ReadArray<byte>());
                var length = reader.Read<int>();
                byte[] array = Encode(reader.ReadArray<byte>());
                buffer.Add(key, new KeyValuePair<int, byte[]>(length, array));
            }

            return buffer;
        }


        private void WriteBuffer (Dictionary<string, string> buffer, SaveWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write(Encoding.Default.GetString(Encode(Convert.FromBase64String(buffer[key]))));
            }
        }


        private Dictionary<string, string> ReadStringBuffer (SaveReader reader) {
            var count = reader.Read<int>();

            var buffer = new Dictionary<string, string>();

            for (var i = 0; i < count; i++) {
                buffer.Add(
                    Encoding.UTF8.GetString(reader.ReadArray<byte>()),
                    Convert.ToBase64String(Encode(Encoding.Default.GetBytes(reader.ReadString())))
                );
            }

            return buffer;
        }


        private void WriteBuffer (Dictionary<string, MeshData> buffer, SaveWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write(buffer[key]);
            }
        }


        private Dictionary<string, MeshData> ReadMeshDataBuffer (SaveReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, MeshData>();

            for (var i = 0; i < count; i++)
                buffer.Add(Encoding.UTF8.GetString(reader.ReadArray<byte>()), reader.ReadMeshData());

            return buffer;
        }


        private byte[] Encode (byte[] bytes) {
            var encodedBytes = new byte[bytes.Length];
            Array.Copy(bytes, encodedBytes, encodedBytes.Length);

            for (int i = 0, j = 0; i < bytes.Length; i++, j++) {
                if (j == m_encodingKey.Length)
                    j = 0;
                encodedBytes[i] ^= m_encodingKey[j];
            }

            return encodedBytes;
        }

    }

}