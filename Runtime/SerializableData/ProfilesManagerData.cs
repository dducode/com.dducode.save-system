using System;
using SaveSystemPackage.Serialization;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct ProfilesManagerData : ISaveData, IBinarySerializable {

        public Map<string, string> profilesMap;


        public void WriteBinary (SaveWriter writer) {
            writer.Write(profilesMap.Count);

            foreach ((string key, string value) in profilesMap) {
                writer.Write(key);
                writer.Write(value);
            }
        }


        public void ReadBinary (SaveReader reader) {
            var count = reader.Read<int>();
            profilesMap = new Map<string, string>();
            for (var i = 0; i < count; i++)
                profilesMap.Add(reader.ReadString(), reader.ReadString());
        }

    }

}