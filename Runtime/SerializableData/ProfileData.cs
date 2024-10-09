using System;
using SaveSystemPackage.Serialization;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct ProfileData : ISaveData, IBinarySerializable {

        public string id;
        public string name;
        public string iconId;


        public void WriteBinary (SaveWriter writer) {
            writer.Write(id);
            writer.Write(name);
            writer.Write(iconId);
        }


        public void ReadBinary (SaveReader reader) {
            id = reader.ReadString();
            name = reader.ReadString();
            iconId = reader.ReadString();
        }

    }

}