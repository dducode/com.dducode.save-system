using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemPackage.Storages {

    public interface IDataStorage {

        public Task Write (string key, byte[] data, CancellationToken token = default);
        public Task<byte[]> Read (string key, CancellationToken token = default);
        public Task Delete (string key);
        public Task Clear ();
        public Task<bool> Exists (string key);

    }

}