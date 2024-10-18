using System;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public class KeyMapItem : ISaveData {

        public string assemblyName;
        public Map<string, string> types;

    }

}