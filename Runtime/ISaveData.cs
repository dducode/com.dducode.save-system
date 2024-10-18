using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace SaveSystemPackage {

    public interface ISaveData {

        [YamlIgnore, JsonIgnore]
        public bool IsEmpty => false;

    }

}