using System;

namespace SaveSystemPackage.Providers {

  public class KeyStore : IKeyProvider {

    private readonly KeyMap _keyMap;

    public KeyStore(KeyMap keyMap) {
      _keyMap = keyMap;
    }

    public string Provide<TData>() where TData : ISaveData {
      Type type = typeof(TData);
      if (!_keyMap.TryGetValue(type, out string key))
        throw new InvalidOperationException($"Key store doesn't contain a key for type \"{type.FullName}\"");
      return key;
    }

    public string Provide<TData>(string prefix) where TData : ISaveData {
      return $"{prefix}_{Provide<TData>()}";
    }

  }

}