using System;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct ProfileData : ISaveData {

        public string id;
        public string name;
        public string iconId;

    }

}