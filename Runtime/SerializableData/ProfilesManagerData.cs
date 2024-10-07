using System;
using System.Collections.Generic;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct ProfilesManagerData : ISaveData {

        public Dictionary<string, string> profilesMap;

    }

}