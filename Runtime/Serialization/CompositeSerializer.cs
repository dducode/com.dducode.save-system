using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Compressing;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Serialization {

    public class CompositeSerializer : ISerializer {

        private readonly ISerializer m_baseSerializer;
        private readonly Cryptographer m_cryptographer;
        private readonly FileCompressor m_compressor;


        public CompositeSerializer (
            ISerializer baseSerializer, Cryptographer cryptographer, FileCompressor compressor
        ) {
            m_baseSerializer = baseSerializer;
            m_cryptographer = cryptographer;
            m_compressor = compressor;
        }


        public async Task<byte[]> Serialize<TData> (TData data, CancellationToken token) where TData : ISaveData {
            token.ThrowIfCancellationRequested();
            byte[] serializedData = await m_baseSerializer.Serialize(data, token);
            byte[] compressedData = m_compressor.Compress(serializedData);
            return m_cryptographer.Encrypt(compressedData);
        }


        public async Task<TData> Deserialize<TData> (byte[] data, CancellationToken token) where TData : ISaveData {
            token.ThrowIfCancellationRequested();
            byte[] decryptedData = m_cryptographer.Decrypt(data);
            byte[] decompressedData = m_compressor.Decompress(decryptedData);
            return await m_baseSerializer.Deserialize<TData>(decompressedData, token);
        }

    }

}