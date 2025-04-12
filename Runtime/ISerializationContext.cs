using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Providers;

namespace SaveSystemPackage {

  public interface ISerializationContext {

    public ISaveDataProvider DataProvider { get; set; }
    public Task SaveData<TData>(TData data, string key = null, CancellationToken token = default) where TData : ISaveData;
    public Task<TData> LoadData<TData>(string key = null, CancellationToken token = default) where TData : ISaveData;
    public Task DeleteData<TData>(string key = null, CancellationToken token = default) where TData : ISaveData;

  }

}