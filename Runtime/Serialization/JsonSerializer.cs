using System.Text;
using UnityEngine;

namespace SaveSystemPackage.Serialization {

    public class JsonSerializer : ISerializer {

        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            return JsonUtility.FromJson<TData>(Encoding.UTF8.GetString(data));
        }


        public string GetFormatCode () {
            return "json";
        }

    }

}