using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemPackage.Storages {

    public interface IDataStorage {

        public Task Write ([NotNull] string key, byte[] data, CancellationToken token = default);
        public Task<byte[]> Read ([NotNull] string key, CancellationToken token = default);
        public Task Delete ([NotNull] string key);
        public Task Clear ();
        public Task<bool> Exists ([NotNull] string key);

    }

}