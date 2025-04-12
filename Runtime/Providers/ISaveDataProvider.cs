using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SaveSystemPackage.Serialization;
using SaveSystemPackage.Storages;

namespace SaveSystemPackage.Providers {

  public interface ISaveDataProvider {

    public IKeyProvider KeyProvider { get; set; }
    public ISerializer Serializer { get; set; }
    public IDataStorage DataStorage { get; set; }
    public string Version { get; set; }
    public IComparer<string> VersionsComparer { get; set; }

    public Task SaveData<TData>([NotNull] TData data, string key = null, CancellationToken token = default) where TData : ISaveData;
    public Task<TData> LoadData<TData>(string key = null, CancellationToken token = default) where TData : ISaveData;
    public Task DeleteData<TData>(string key = null) where TData : ISaveData;
    public Task DeleteVersions<TData>([NotNull] IEnumerable<string> versions, string key = null) where TData : ISaveData;

  }

}