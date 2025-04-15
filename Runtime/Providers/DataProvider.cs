using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Serialization;
using SaveSystemPackage.Storages;
using UnityEngine;

namespace SaveSystemPackage.Providers {

  public class DataProvider : ISaveDataProvider {

    public ISerializer Serializer {
      get => _serializer;
      set => _serializer = value ?? throw new ArgumentNullException(nameof(Serializer));
    }

    public IDataStorage DataStorage {
      get => _dataStorage;
      set => _dataStorage = value ?? throw new ArgumentNullException(nameof(DataStorage));
    }

    public string Version {
      get => _version;
      set => _version = !string.IsNullOrEmpty(value) ? value : throw new ArgumentNullException(nameof(Version));
    }

    public IComparer<string> VersionsComparer {
      get => _versionsComparer;
      set => _versionsComparer = value ?? throw new ArgumentNullException(nameof(VersionsComparer));
    }

    private ISerializer _serializer;
    private IDataStorage _dataStorage;
    private string _version;
    private IComparer<string> _versionsComparer;

    public DataProvider() {
      Serializer = SaveSystem.Settings.SharedSerializer;
      DataStorage = new FileSystemStorage(Storage.Root, Serializer.GetFormatCode());
      Version = Application.version;
      VersionsComparer = Comparer<string>.Default;
    }

    public async Task SaveData<TData>(string key, TData data, CancellationToken token = default) where TData : ISaveData {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentNullException(nameof(key));
      if (data == null)
        throw new ArgumentNullException(nameof(data));
      if (data.IsEmpty)
        return;

      token.ThrowIfCancellationRequested();
      byte[] bytes = await DataStorage.Read(key, token);

      Map<string, TData> map;

      try {
        map = Serializer.Deserialize<Map<string, TData>>(bytes) ?? new Map<string, TData>();
      }
      catch (Exception) {
        map = new Map<string, TData>(); // support backward compatibility
      }

      map[Version] = data;
      await DataStorage.Write(key, Serializer.Serialize(map), token);
    }

    public async Task<TData> LoadData<TData>(string key, TData defaultData = default, CancellationToken token = default) where TData : ISaveData {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentNullException(nameof(key));

      token.ThrowIfCancellationRequested();
      byte[] bytes = await DataStorage.Read(key, token);
      Map<string, TData> map;

      try {
        map = Serializer.Deserialize<Map<string, TData>>(bytes) ?? new Map<string, TData>();
      }
      catch (Exception) {
        return Serializer.Deserialize<TData>(bytes) ?? defaultData; // support backward compatibility
      }

      if (map.TryGetValue(Version, out TData data))
        return data;

      string latestVersion = map.Keys.OrderByDescending(version => version, VersionsComparer).FirstOrDefault();
      return !string.IsNullOrEmpty(latestVersion) ? map[latestVersion] : defaultData;
    }

    public async Task DeleteData<TData>(string key) where TData : ISaveData {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentNullException(nameof(key));

      await DataStorage.Delete(key);
    }

    public async Task DeleteVersions<TData>(string key, IEnumerable<string> versions) where TData : ISaveData {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentNullException(nameof(key));
      if (versions == null)
        throw new ArgumentNullException(nameof(versions));

      var map = Serializer.Deserialize<Map<string, TData>>(await DataStorage.Read(key, CancellationToken.None));
      foreach (string version in versions)
        map.Remove(version);
      await DataStorage.Write(key, Serializer.Serialize(map));
    }

  }

}