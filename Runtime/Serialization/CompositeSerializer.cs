using SaveSystemPackage.Compressing;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Serialization {

    public class CompositeSerializer : ISerializer {

        private readonly ISerializer m_baseSerializer;
        private readonly IEncryptor m_encryptor;
        private readonly ICompressor m_compressor;


        public CompositeSerializer (
            ISerializer baseSerializer, IEncryptor encryptor, ICompressor compressor
        ) {
            m_baseSerializer = baseSerializer;
            m_encryptor = encryptor;
            m_compressor = compressor;
        }


        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            byte[] serializedData = m_baseSerializer.Serialize(data);
            byte[] compressedData = m_compressor.Compress(serializedData);
            return m_encryptor.Encrypt(compressedData);
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            byte[] decryptedData = m_encryptor.Decrypt(data);
            byte[] decompressedData = m_compressor.Decompress(decryptedData);
            return m_baseSerializer.Deserialize<TData>(decompressedData);
        }


        public string GetFormatCode () {
            return m_baseSerializer.GetFormatCode();
        }

    }

}