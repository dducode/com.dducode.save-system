using System;
using System.Collections.Generic;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    internal struct ProfilesManagerData : ISaveData {

        public Dictionary<string, string> profilesMap;

    }

}