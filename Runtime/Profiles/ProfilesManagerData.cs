using System;
using System.Collections.Generic;

namespace SaveSystemPackage.Profiles {

    [Serializable]
    public struct ProfilesManagerData : ISaveData {

        public Dictionary<string, string> profilesMap;

    }

}