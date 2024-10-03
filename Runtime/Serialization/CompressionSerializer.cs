using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Compressing;

namespace SaveSystemPackage.Serialization {

    public class CompressionSerializer : ISerializer {

        private readonly ISerializer m_baseSerializer;
        private readonly FileCompressor m_compressor;


        public CompressionSerializer (ISerializer baseSerializer, FileCompressor compressor) {
            m_baseSerializer = baseSerializer;
            m_compressor = compressor;
        }


        public async Task<byte[]> Serialize<TData> (TData data, CancellationToken token) where TData : ISaveData {
            token.ThrowIfCancellationRequested();
            byte[] serializedData = await m_baseSerializer.Serialize(data, token);
            return m_compressor.Compress(serializedData);
        }


        public async Task<TData> Deserialize<TData> (byte[] data, CancellationToken token) where TData : ISaveData {
            token.ThrowIfCancellationRequested();
            byte[] decompressedData = m_compressor.Decompress(data);
            return await m_baseSerializer.Deserialize<TData>(decompressedData, token);
        }

    }

}