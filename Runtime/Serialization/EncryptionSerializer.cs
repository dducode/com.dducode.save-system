using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Serialization {

    public class EncryptionSerializer : ISerializer {

        private readonly ISerializer m_baseSerializer;
        private readonly Cryptographer m_cryptographer;


        public EncryptionSerializer (ISerializer baseSerializer, Cryptographer cryptographer) {
            m_baseSerializer = baseSerializer;
            m_cryptographer = cryptographer;
        }


        public async Task<byte[]> Serialize<TData> (TData data, CancellationToken token) where TData : ISaveData {
            byte[] serializedData = await m_baseSerializer.Serialize(data, token);
            return m_cryptographer.Encrypt(serializedData);
        }


        public async Task<TData> Deserialize<TData> (byte[] data, CancellationToken token) where TData : ISaveData {
            byte[] decryptedData = m_cryptographer.Decrypt(data);
            return await m_baseSerializer.Deserialize<TData>(decryptedData, token);
        }

    }

}