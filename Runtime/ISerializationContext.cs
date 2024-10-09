using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemPackage {

    public interface ISerializationContext : IReadOnlyContext {

        public Task SaveData<TData> (TData data, CancellationToken token = default) where TData : ISaveData;


        public Task SaveData<TData> (string key, TData data, CancellationToken token = default)
            where TData : ISaveData;


        public Task DeleteData<TData> () where TData : ISaveData;


        public Task DeleteData<TData> (string key) where TData : ISaveData;

    }

}