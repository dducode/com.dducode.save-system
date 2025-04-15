using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SaveSystemPackage.Serialization;
using SaveSystemPackage.Storages;

namespace SaveSystemPackage.Providers {

  public interface ISaveDataProvider {

    public ISerializer Serializer { get; set; }
    public IDataStorage DataStorage { get; set; }
    public string Version { get; set; }
    public IComparer<string> VersionsComparer { get; set; }

    public Task SaveData<TData>([NotNull] string key, [NotNull] TData data, CancellationToken token = default) where TData : ISaveData;
    public Task<TData> LoadData<TData>([NotNull] string key, TData defaultData = default, CancellationToken token = default) where TData : ISaveData;
    public Task DeleteData<TData>([NotNull] string key) where TData : ISaveData;
    public Task DeleteVersions<TData>([NotNull] string key, [NotNull] IEnumerable<string> versions) where TData : ISaveData;

  }

}