using System;
using System.Collections.Generic;

namespace SaveSystemPackage.Providers {

    public class KeyStore : IKeyProvider {

        private readonly Dictionary<Type, string> m_keyMap;


        public KeyStore (Dictionary<Type, string> keyMap) {
            m_keyMap = keyMap;
        }


        public string Provide<TData> () where TData : ISaveData {
            Type type = typeof(TData);
            if (!m_keyMap.ContainsKey(type))
                throw new InvalidOperationException($"Key store doesn't contain a key for type \"{type.Name}\"");
            return m_keyMap[type];
        }


        public string Provide<TData> (string prefix) where TData : ISaveData {
            return $"{prefix}_{Provide<TData>()}";
        }

    }

}