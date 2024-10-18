using System;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct ProfilesManagerData : ISaveData {

        public Map<string, string> profilesMap;

        [YamlIgnore, JsonIgnore]
        public bool IsEmpty => profilesMap.Count == 0;

    }

}