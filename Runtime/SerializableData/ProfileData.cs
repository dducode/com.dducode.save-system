using System;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct ProfileData : ISaveData {

        public string id;
        public string name;
        public string iconId;

        [YamlIgnore, JsonIgnore]
        public bool IsEmpty =>
            string.IsNullOrEmpty(id)
            && string.IsNullOrEmpty(name)
            && string.IsNullOrEmpty(iconId);

    }

}