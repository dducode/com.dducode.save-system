using SaveSystemPackage.Security;

namespace SaveSystemPackage.Serialization {

    public class EncryptionSerializer : ISerializer {

        private readonly ISerializer m_baseSerializer;
        private readonly IEncryptor m_encryptor;


        public EncryptionSerializer (ISerializer baseSerializer, IEncryptor encryptor) {
            m_baseSerializer = baseSerializer;
            m_encryptor = encryptor;
        }


        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            byte[] serializedData = m_baseSerializer.Serialize(data);
            return m_encryptor.Encrypt(serializedData);
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            byte[] decryptedData = m_encryptor.Decrypt(data);
            return m_baseSerializer.Deserialize<TData>(decryptedData);
        }


        public string GetFormatCode () {
            return m_baseSerializer.GetFormatCode();
        }

    }

}