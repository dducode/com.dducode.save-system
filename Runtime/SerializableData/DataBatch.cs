using System;
using SaveSystemPackage.Serialization;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public class DataBatch<TData> : ISaveData, IBinarySerializable where TData : ISaveData {

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
            var binarySerializer = new BinarySerializer();

            foreach ((string key, TData value) in batch) {
                writer.Write(key);
                writer.Write(binarySerializer.Serialize(value));
            }
        }


        public void ReadBinary (SaveReader reader) {
            var count = reader.Read<int>();
            batch = new Map<string, TData>();
            var binarySerializer = new BinarySerializer();
            for (var i = 0; i < count; i++)
                batch.Add(reader.ReadString(), binarySerializer.Deserialize<TData>(reader.ReadArray<byte>()));
        }

    }

}