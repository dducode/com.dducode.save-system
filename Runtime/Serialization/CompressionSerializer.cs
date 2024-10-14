using SaveSystemPackage.Compressing;

namespace SaveSystemPackage.Serialization {

    public class CompressionSerializer : ISerializer {

        private readonly ISerializer m_baseSerializer;
        private readonly ICompressor m_compressor;


        public CompressionSerializer (ISerializer baseSerializer, ICompressor compressor) {
            m_baseSerializer = baseSerializer;
            m_compressor = compressor;
        }


        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            byte[] serializedData = m_baseSerializer.Serialize(data);
            return m_compressor.Compress(serializedData);
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            byte[] decompressedData = m_compressor.Decompress(data);
            return m_baseSerializer.Deserialize<TData>(decompressedData);
        }


        public string GetFormatCode () {
            return m_baseSerializer.GetFormatCode();
        }

    }

}