﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;

// ReSharper disable UnusedMember.Global
namespace SaveSystem {

    /// <summary>
    /// The buffer for writing and reading data
    /// </summary>
    public record DataBuffer {

        private readonly Dictionary<string, byte[]> m_commonBuffer;
        private readonly Dictionary<string, KeyValuePair<int, byte[]>> m_arrayBuffer;
        private readonly Dictionary<string, MeshData> m_meshDataBuffer;
        private readonly Dictionary<string, string> m_stringBuffer;


        public DataBuffer () {
            m_commonBuffer = new Dictionary<string, byte[]>();
            m_arrayBuffer = new Dictionary<string, KeyValuePair<int, byte[]>>();
            m_meshDataBuffer = new Dictionary<string, MeshData>();
            m_stringBuffer = new Dictionary<string, string>();
        }


        internal DataBuffer (BinaryReader reader) {
            m_commonBuffer = ReadCommonBuffer(reader);
            m_arrayBuffer = ReadArrayBuffer(reader);
            m_meshDataBuffer = ReadMeshDataBuffer(reader);
            m_stringBuffer = ReadStringBuffer(reader);
        }


        public void Add<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            byte[] data = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToArray();
            m_commonBuffer.Add(key, data);
        }


        public void Add<TArray> ([NotNull] string key, [NotNull] TArray[] array) where TArray : unmanaged {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var data = new KeyValuePair<int, byte[]>(
                array.Length,
                MemoryMarshal.AsBytes((ReadOnlySpan<TArray>)array).ToArray()
            );
            m_arrayBuffer.Add(key, data);
        }


        public void Add ([NotNull] string key, MeshData value) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_meshDataBuffer.Add(key, value);
        }


        public void Add ([NotNull] string key, [NotNull] string value) {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_stringBuffer.Add(key, value);
        }


        public TValue Get<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_commonBuffer.ContainsKey(key) ? MemoryMarshal.Read<TValue>(m_commonBuffer[key]) : default;
        }


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


        public MeshData GetMeshData ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_meshDataBuffer.ContainsKey(key) ? m_meshDataBuffer[key] : default;
        }


        public string GetString ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_stringBuffer.ContainsKey(key) ? m_stringBuffer[key] : default;
        }


        internal void WriteData (BinaryWriter writer) {
            WriteBuffer(m_commonBuffer, writer);
            WriteBuffer(m_arrayBuffer, writer);
            WriteBuffer(m_meshDataBuffer, writer);
            WriteBuffer(m_stringBuffer, writer);
        }


        private void WriteBuffer (Dictionary<string, byte[]> buffer, BinaryWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(key);
                writer.Write(buffer[key]);
            }
        }


        private Dictionary<string, byte[]> ReadCommonBuffer (BinaryReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, byte[]>();

            for (var i = 0; i < count; i++)
                buffer.Add(reader.ReadString(), reader.ReadArray<byte>());

            return buffer;
        }


        private void WriteBuffer (Dictionary<string, KeyValuePair<int, byte[]>> buffer, BinaryWriter writer) {
            writer.Write(buffer.Count);

            foreach (string key in buffer.Keys) {
                writer.Write(key);
                writer.Write(buffer[key].Key);
                writer.Write(buffer[key].Value);
            }
        }


        private Dictionary<string, KeyValuePair<int, byte[]>> ReadArrayBuffer (BinaryReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, KeyValuePair<int, byte[]>>();

            for (var i = 0; i < count; i++) {
                string key = reader.ReadString();
                var length = reader.Read<int>();
                byte[] array = reader.ReadArray<byte>();
                buffer.Add(key, new KeyValuePair<int, byte[]>(length, array));
            }

            return buffer;
        }


        private void WriteBuffer (Dictionary<string, MeshData> buffer, BinaryWriter writer) {
            writer.Write(buffer.Count);
            writer.Write(buffer.Keys.ToArray());
            writer.Write(buffer.Values.ToArray());
        }


        private Dictionary<string, MeshData> ReadMeshDataBuffer (BinaryReader reader) {
            var count = reader.Read<int>();
            ReadOnlySpan<string> keys = reader.ReadStringArray();
            ReadOnlySpan<MeshData> values = reader.ReadMeshDataArray();

            var buffer = new Dictionary<string, MeshData>();
            for (var i = 0; i < count; i++)
                buffer.Add(keys[i], values[i]);
            return buffer;
        }


        private void WriteBuffer (Dictionary<string, string> buffer, BinaryWriter writer) {
            writer.Write(buffer.Count);
            writer.Write(buffer.Keys.ToArray());
            writer.Write(buffer.Values.ToArray());
        }


        private Dictionary<string, string> ReadStringBuffer (BinaryReader reader) {
            var count = reader.Read<int>();
            ReadOnlySpan<string> keys = reader.ReadStringArray();
            ReadOnlySpan<string> values = reader.ReadStringArray();

            var buffer = new Dictionary<string, string>();
            for (var i = 0; i < count; i++)
                buffer.Add(keys[i], values[i]);
            return buffer;
        }

    }

}