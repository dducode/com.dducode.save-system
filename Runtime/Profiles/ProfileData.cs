using System;

namespace SaveSystemPackage.Profiles {

    [Serializable]
    public struct ProfileData : ISaveData {

        public string id;
        public string name;
        public string iconId;

    }

}