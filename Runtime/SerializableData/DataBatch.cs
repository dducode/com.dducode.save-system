using System;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public class DataBatch<TData> : Map<string, TData>, ISaveData where TData : ISaveData {

        [YamlIgnore, JsonIgnore]
        public bool IsEmpty => Count == 0;

    }

}