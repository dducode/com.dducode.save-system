using System;
using SaveSystemPackage.Serialization;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public class DataBatch<TData> : ISaveData, IBinarySerializable where TData : unmanaged, ISaveData {

        public Map<string, TData> batch = new();

        public TData this [string key] => batch[key];


        public void Add (string key, TData item) {
            batch.Add(key, item);
        }


        public bool ContainsKey (string key) {
            return batch.ContainsKey(key);
        }


        public void WriteBinary (SaveWriter writer) {
            writer.Write(batch.Count);

            foreach ((string key, TData value) in batch) {
                writer.Write(key);
                writer.Write(value);
            }
        }


        public void ReadBinary (SaveReader reader) {
            var count = reader.Read<int>();
            batch = new Map<string, TData>();
            for (var i = 0; i < count; i++)
                batch.Add(reader.ReadString(), reader.Read<TData>());
        }

    }

}