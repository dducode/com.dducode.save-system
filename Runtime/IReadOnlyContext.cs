using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Providers;
using SaveSystemPackage.Serialization;
using SaveSystemPackage.Storages;

namespace SaveSystemPackage {

    public interface IReadOnlyContext {

        public ISerializer Serializer { get; set; }
        public IKeyProvider KeyProvider { get; set; }
        public IDataStorage DataStorage { get; set; }


        public Task<TData> LoadData<TData> (TData @default = default, CancellationToken token = default)
            where TData : ISaveData;


        public Task<TData> LoadData<TData> (string key, TData @default = default, CancellationToken token = default)
            where TData : ISaveData;

    }

}