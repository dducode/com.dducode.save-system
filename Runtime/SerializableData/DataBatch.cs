using System;
using System.Collections.Generic;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public class DataBatch<TData> : ISaveData where TData : ISaveData {

        public Dictionary<string, TData> batch = new();

        public TData this [string key] => batch[key];


        public void Add (string key, TData item) {
            batch.Add(key, item);
        }


        public bool ContainsKey (string key) {
            return batch.ContainsKey(key);
        }

    }

}