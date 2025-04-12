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

    public IKeyProvider KeyProvider {
      get => _keyProvider;
      set => _keyProvider = value ?? throw new ArgumentNullException(nameof(KeyProvider));
    }

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

    private IKeyProvider _keyProvider;
    private ISerializer _serializer;
    private IDataStorage _dataStorage;
    private string _version;
    private IComparer<string> _versionsComparer;

    public DataProvider() {
      KeyProvider = new KeyStore(KeyMap.PredefinedMap);
      Serializer = SaveSystem.Settings.SharedSerializer;
      DataStorage = new FileSystemStorage(Storage.Root, Serializer.GetFormatCode());
      Version = Application.version;
      VersionsComparer = Comparer<string>.Default;
    }

    public async Task SaveData<TData>(TData data, string key = null, CancellationToken token = default) where TData : ISaveData {
      if (data == null)
        throw new ArgumentNullException(nameof(data));
      if (data.IsEmpty)
        return;

      token.ThrowIfCancellationRequested();
      string providedKey = GetKey<TData>(key);
      byte[] bytes = await DataStorage.Read(providedKey, token);

      Map<string, TData> map;
      try {
        map = Serializer.Deserialize<Map<string, TData>>(bytes) ?? new Map<string, TData>();
      }
      catch (Exception) {
        map = new Map<string, TData>(); // support backward compatibility
      }

      map[Version] = data;
      await DataStorage.Write(providedKey, Serializer.Serialize(map), token);
    }

    public async Task<TData> LoadData<TData>(string key = null, CancellationToken token = default) where TData : ISaveData {
      token.ThrowIfCancellationRequested();
      byte[] bytes = await DataStorage.Read(GetKey<TData>(key), token);

      Map<string, TData> map;
      try {
        map = Serializer.Deserialize<Map<string, TData>>(bytes) ?? new Map<string, TData>();
      }
      catch (Exception) {
        return Serializer.Deserialize<TData>(bytes); // support backward compatibility
      }

      if (map.TryGetValue(Version, out TData data))
        return data;

      string latestVersion = map.Keys.OrderByDescending(version => version, VersionsComparer).FirstOrDefault();
      return string.IsNullOrEmpty(latestVersion) ? default : map[latestVersion];
    }

    public async Task DeleteData<TData>(string key = null) where TData : ISaveData {
      await DataStorage.Delete(GetKey<TData>(key));
    }

    public async Task DeleteVersions<TData>(IEnumerable<string> versions, string key = null) where TData : ISaveData {
      if (versions == null)
        throw new ArgumentNullException(nameof(versions));

      string providedKey = GetKey<TData>(key);
      var map = Serializer.Deserialize<Map<string, TData>>(await DataStorage.Read(providedKey));
      if (map == null)
        return;

      foreach (string version in versions)
        map.Remove(version);
      await DataStorage.Write(providedKey, Serializer.Serialize(map));
    }

    private string GetKey<TData>(string key) where TData : ISaveData {
      return string.IsNullOrEmpty(key) ? KeyProvider.Provide<TData>() : KeyProvider.Provide<TData>(key);
    }

  }

}