using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UnityEngine;

namespace SaveSystemPackage.Serialization {

    public class JsonSerializer : ISerializer {

        public byte[] Serialize<TData> ([NotNull] TData data) where TData : ISaveData {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return data.IsEmpty ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            if (data == null || data.Length == 0)
                return default;
            return JsonUtility.FromJson<TData>(Encoding.UTF8.GetString(data));
        }


        public string GetFormatCode () {
            return CodeFormats.JSON;
        }

    }

}