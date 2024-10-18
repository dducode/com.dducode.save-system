using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Serialization {

    public class BinarySerializer : ISerializer {

        public byte[] Serialize<TData> ([NotNull] TData data) where TData : ISaveData {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.IsEmpty)
                return Array.Empty<byte>();

            using var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            if (data == null || data.Length == 0)
                return default;

            using var stream = new MemoryStream(data);
            var formatter = new BinaryFormatter();
            return (TData)formatter.Deserialize(stream);
        }


        public string GetFormatCode () {
            return CodeFormats.Binary;
        }

    }

}