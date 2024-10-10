using SaveSystemPackage.Security;

namespace SaveSystemPackage.Serialization {

    public class EncryptionSerializer : ISerializer {

        private readonly ISerializer m_baseSerializer;
        private readonly Cryptographer m_cryptographer;


        public EncryptionSerializer (ISerializer baseSerializer, Cryptographer cryptographer) {
            m_baseSerializer = baseSerializer;
            m_cryptographer = cryptographer;
        }


        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            byte[] serializedData = m_baseSerializer.Serialize(data);
            return m_cryptographer.Encrypt(serializedData);
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            byte[] decryptedData = m_cryptographer.Decrypt(data);
            return m_baseSerializer.Deserialize<TData>(decryptedData);
        }


        public string GetFormatCode () {
            return m_baseSerializer.GetFormatCode();
        }

    }

}