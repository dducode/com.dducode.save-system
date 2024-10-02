using System;
using System.Collections.Generic;
using SaveSystemPackage.ComponentsRecording;

namespace SaveSystemPackage.Providers {

    public class KeyStore : IKeyProvider {

        private readonly Dictionary<Type, string> m_map = new() {
            {typeof(TransformData), "transform-data"},
            {typeof(RigidbodyData), "rigidbody-data"},
            {typeof(MeshData), "mesh-data"}
        };


        public void ConcatMap (Dictionary<Type, string> map) {
            foreach ((Type key, string value) in map)
                m_map.TryAdd(key, value);
        }


        public void AddKey<TData> (string key) where TData : ISaveData {
            m_map.TryAdd(typeof(TData), key);
        }


        public string GetKey<TData> () where TData : ISaveData {
            return m_map[typeof(TData)];
        }

    }

}