using System;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct ProfilesManagerData : ISaveData {

        public Map<string, string> profilesMap;

    }

}