using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemPackage.Serialization {

    public class BinarySerializer : ISerializer {

        public Task<byte[]> Serialize<TData> (TData data, CancellationToken token) where TData : ISaveData {
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return Task.FromResult(stream.ToArray());
        }


        public Task<TData> Deserialize<TData> (byte[] data, CancellationToken token) where TData : ISaveData {
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream(data);
            return Task.FromResult((TData)formatter.Deserialize(stream));
        }

    }

}