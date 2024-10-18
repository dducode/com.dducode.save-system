using System;

namespace SaveSystemPackage.Providers {

    public class KeyStore : IKeyProvider {

        private readonly KeyMap m_keyMap;


        public KeyStore (KeyMap keyMap) {
            m_keyMap = keyMap;
        }


        public string Provide<TData> () where TData : ISaveData {
            Type type = typeof(TData);
            if (!m_keyMap.ContainsKey(type))
                throw new InvalidOperationException($"Key store doesn't contain a key for type \"{type.FullName}\"");
            return m_keyMap[type];
        }


        public string Provide<TData> (string prefix) where TData : ISaveData {
            return $"{prefix}_{Provide<TData>()}";
        }

    }

}