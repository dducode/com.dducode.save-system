using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemPackage.Serialization {

    public interface ISerializer {

        public Task<byte[]> Serialize<TData> (TData data, CancellationToken token) where TData : ISaveData;
        public Task<TData> Deserialize<TData> (byte[] data, CancellationToken token) where TData : ISaveData;

    }

}