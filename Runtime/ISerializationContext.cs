using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SaveSystemPackage.Providers;

namespace SaveSystemPackage {

  public interface ISerializationContext {

    public ISaveDataProvider DataProvider { get; set; }

    public Task SaveData<TData>([NotNull] string key, TData data, CancellationToken token = default) where TData : ISaveData;
    public Task<TData> LoadData<TData>([NotNull] string key, TData defaultData = default, CancellationToken token = default) where TData : ISaveData;
    public Task DeleteData<TData>([NotNull] string key, CancellationToken token = default) where TData : ISaveData;

  }

}