using System;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public class DataBatch<TData> : Map<string, TData>, ISaveData where TData : ISaveData { }

}