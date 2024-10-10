using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Serialization;

// ReSharper disable UnusedMember.Global
namespace SaveSystemPackage {

    /// <summary>
    /// The buffer for writing and reading data
    /// </summary>
    public record DataBuffer {

        protected readonly Dictionary<string, byte[]> commonBuffer;

        public int Count => commonBuffer.Count;
        public bool HasChanges { get; protected set; }


        public DataBuffer () {
            commonBuffer = new Dictionary<string, byte[]>();
        }


        internal DataBuffer (SaveReader reader) {
            var count = reader.Read<int>();
            var buffer = new Dictionary<string, byte[]>();

            for (var i = 0; i < count; i++)
                buffer.Add(Encoding.UTF8.GetString(reader.ReadArray<byte>()), reader.ReadArray<byte>());

            commonBuffer = buffer;
        }


        public virtual void Write<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            commonBuffer[key] = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToArray();
            HasChanges = true;
        }


        [Pure]
        public virtual TValue Read<TValue> ([NotNull] string key, TValue defaultValue = default) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return commonBuffer.TryGetValue(key, out byte[] value)
                ? MemoryMarshal.Read<TValue>(value)
                : defaultValue;
        }


        public virtual void Write<TArray> ([NotNull] string key, [NotNull] TArray[] array) where TArray : unmanaged {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            int length = array.Length;
            byte[] arrayLength = MemoryMarshal.AsBytes(
                MemoryMarshal.CreateReadOnlySpan(ref length, 1)
            ).ToArray();
            byte[] data = MemoryMarshal.AsBytes((ReadOnlySpan<TArray>)array).ToArray();

            commonBuffer[key] = arrayLength.Concat(data).ToArray();
            HasChanges = true;
        }


        [Pure]
        public virtual TArray[] ReadArray<TArray> ([NotNull] string key) where TArray : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (commonBuffer.TryGetValue(key, out byte[] value)) {
                (byte[] length, byte[] data) split = value.Split(sizeof(int));
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


        public virtual void Write ([NotNull] string key, [NotNull] string value) {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            commonBuffer[key] = Encoding.Default.GetBytes(value);
            HasChanges = true;
        }


        [Pure]
        public virtual string ReadString ([NotNull] string key, string defaultValue = null) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return commonBuffer.TryGetValue(key, out byte[] value)
                ? Encoding.Default.GetString(value)
                : defaultValue;
        }


        public bool Delete ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return commonBuffer.Remove(key);
        }


        public bool ContainsKey ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return commonBuffer.ContainsKey(key);
        }


        internal virtual void WriteData (SaveWriter writer) {
            writer.Write(commonBuffer.Count);

            foreach (string key in commonBuffer.Keys) {
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write(commonBuffer[key]);
            }

            HasChanges = false;
        }


        internal void Clear () {
            commonBuffer.Clear();
        }

    }

}