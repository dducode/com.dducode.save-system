using System;
using System.Diagnostics.CodeAnalysis;
using SaveSystemPackage.Compressing;

namespace SaveSystemPackage.Serialization {

    public class CompressionSerializer : ISerializer {

        private readonly ISerializer m_baseSerializer;
        private readonly ICompressor m_compressor;


        public CompressionSerializer (ISerializer baseSerializer, ICompressor compressor) {
            m_baseSerializer = baseSerializer;
            m_compressor = compressor;
        }


        public byte[] Serialize<TData> ([NotNull] TData data) where TData : ISaveData {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.IsEmpty)
                return Array.Empty<byte>();

            byte[] serializedData = m_baseSerializer.Serialize(data);
            return m_compressor.Compress(serializedData);
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            if (data == null || data.Length == 0)
                return default;

            byte[] decompressedData = m_compressor.Decompress(data);
            return m_baseSerializer.Deserialize<TData>(decompressedData);
        }


        public string GetFormatCode () {
            return m_baseSerializer.GetFormatCode();
        }

    }

}