using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SaveSystemPackage.Extensions;
using SaveSystemPackage.Serialization;

// ReSharper disable UnusedMember.Global
namespace SaveSystemPackage {

    /// <summary>
    /// The buffer for writing and reading data
    /// </summary>
    public record DataBuffer {

        private readonly Dictionary<string, byte[]> m_commonBuffer;
        private readonly byte[] m_encodingKey = GenerateKey();

        public int Count => m_commonBuffer.Count;
        public bool HasChanges { get; private set; }


        public DataBuffer () {
            m_commonBuffer = new Dictionary<string, byte[]>();
        }


        internal DataBuffer (SaveReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, byte[]>();

            for (var i = 0; i < count; i++)
                buffer.Add(Encoding.UTF8.GetString(reader.ReadArray<byte>()), Encode(reader.ReadArray<byte>()));

            m_commonBuffer = buffer;
        }


        private static byte[] GenerateKey () {
            var key = new byte[16];
            RandomNumberGenerator.Fill(key);
            return key;
        }


        public void Write<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

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

            int length = array.Length;
            byte[] arrayLength = MemoryMarshal.AsBytes(
                MemoryMarshal.CreateReadOnlySpan(ref length, 1)
            ).ToArray();
            byte[] data = MemoryMarshal.AsBytes((ReadOnlySpan<TArray>)array).ToArray();

            m_commonBuffer[key] = Encode(arrayLength.Concat(data).ToArray());
            HasChanges = true;
        }


        [Pure]
        public TArray[] ReadArray<TArray> ([NotNull] string key) where TArray : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (m_commonBuffer.TryGetValue(key, out byte[] value)) {
                (byte[] length, byte[] data) split = Encode(value).Split(sizeof(int));
                var array = new TArray[MemoryMarshal.Read<int>(split.length)];
                Span<byte> span = MemoryMarshal.AsBytes((Span<TArray>)array);
                for (var i = 0; i < span.Length; i++)
                    span[i] = split.data[i];
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

            m_commonBuffer[key] = Encode(Encoding.Default.GetBytes(value));
            HasChanges = true;
        }


        [Pure]
        public string ReadString ([NotNull] string key, string defaultValue = null) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_commonBuffer.TryGetValue(key, out byte[] value)
                ? Encoding.Default.GetString(Encode(value))
                : defaultValue;
        }


        public bool Delete ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_commonBuffer.Remove(key);
        }


        public bool ContainsKey ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_commonBuffer.ContainsKey(key);
        }


        internal void WriteData (SaveWriter writer) {
            writer.Write(m_commonBuffer.Count);

            foreach (string key in m_commonBuffer.Keys) {
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write(Encode(m_commonBuffer[key]));
            }

            HasChanges = false;
        }


        internal void Clear () {
            m_commonBuffer.Clear();
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