using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemPackage.Storages {

    public interface IDataStorage {

        public Task WriteData (string key, byte[] data, CancellationToken token);
        public Task<byte[]> ReadData (string key, CancellationToken token);

    }

}