using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;
using SaveSystemPackage.BinaryHandlers;

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

        public int Count => m_commonBuffer.Count + m_arrayBuffer.Count + m_stringBuffer.Count + m_meshDataBuffer.Count;


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


        public void Write<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_commonBuffer[key] = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToArray();
        }


        public void Write<TArray> ([NotNull] string key, [NotNull] TArray[] array) where TArray : unmanaged {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_arrayBuffer[key] = new KeyValuePair<int, byte[]>(
                array.Length,
                MemoryMarshal.AsBytes((ReadOnlySpan<TArray>)array).ToArray()
            );
        }


        public void Write ([NotNull] string key, [NotNull] string value) {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_stringBuffer[key] = value;
        }


        public void Write ([NotNull] string key, MeshData value) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_meshDataBuffer[key] = value;
        }


        [Pure]
        public TValue Get<TValue> ([NotNull] string key, TValue defaultValue = default) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_commonBuffer.TryGetValue(key, out byte[] value) ? MemoryMarshal.Read<TValue>(value) : defaultValue;
        }


        [Pure]
        public TArray[] GetArray<TArray> ([NotNull] string key) where TArray : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (m_arrayBuffer.ContainsKey(key)) {
                var array = new TArray[m_arrayBuffer[key].Key];
                Span<byte> span = MemoryMarshal.AsBytes((Span<TArray>)array);
                byte[] data = m_arrayBuffer[key].Value;
                for (var i = 0; i < span.Length; i++)
                    span[i] = data[i];
                return array;
            }
            else {
                return Array.Empty<TArray>();
            }
        }


        [Pure]
        public string GetString ([NotNull] string key, string defaultValue) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_stringBuffer.GetValueOrDefault(key, defaultValue);
        }


        [Pure]
        public MeshData GetMeshData ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_meshDataBuffer.GetValueOrDefault(key);
        }


        internal void WriteData (SaveWriter writer) {
            WriteBuffer(m_commonBuffer, writer);
            WriteBuffer(m_arrayBuffer, writer);
            WriteBuffer(m_stringBuffer, writer);
            WriteBuffer(m_meshDataBuffer, writer);
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
                writer.Write(Encoding.ASCII.GetBytes(key));
                writer.Write(buffer[key]);
            }
        }


        private Dictionary<string, byte[]> ReadCommonBuffer (SaveReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, byte[]>();

            for (var i = 0; i < count; i++)
                buffer.Add(Encoding.ASCII.GetString(reader.ReadArray<byte>()), reader.ReadArray<byte>());

            return buffer;
        }


        private void WriteBuffer (Dictionary<string, KeyValuePair<int, byte[]>> buffer, SaveWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(Encoding.ASCII.GetBytes(key));
                writer.Write(buffer[key].Key);
                writer.Write(buffer[key].Value);
            }
        }


        private Dictionary<string, KeyValuePair<int, byte[]>> ReadArrayBuffer (SaveReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, KeyValuePair<int, byte[]>>();

            for (var i = 0; i < count; i++) {
                string key = Encoding.ASCII.GetString(reader.ReadArray<byte>());
                var length = reader.Read<int>();
                byte[] array = reader.ReadArray<byte>();
                buffer.Add(key, new KeyValuePair<int, byte[]>(length, array));
            }

            return buffer;
        }


        private void WriteBuffer (Dictionary<string, MeshData> buffer, SaveWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(Encoding.ASCII.GetBytes(key));
                writer.Write(buffer[key]);
            }
        }


        private Dictionary<string, MeshData> ReadMeshDataBuffer (SaveReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, MeshData>();

            for (var i = 0; i < count; i++)
                buffer.Add(Encoding.ASCII.GetString(reader.ReadArray<byte>()), reader.ReadMeshData());

            return buffer;
        }


        private void WriteBuffer (Dictionary<string, string> buffer, SaveWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(Encoding.ASCII.GetBytes(key));
                writer.Write(buffer[key]);
            }
        }


        private Dictionary<string, string> ReadStringBuffer (SaveReader reader) {
            var count = reader.Read<int>();

            var buffer = new Dictionary<string, string>();
            for (var i = 0; i < count; i++)
                buffer.Add(Encoding.ASCII.GetString(reader.ReadArray<byte>()), reader.ReadString());

            return buffer;
        }

    }

}