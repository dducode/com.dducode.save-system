using SaveSystemPackage.Serialization;

namespace SaveSystemPackage {

    public interface IBinarySerializable {

        public void WriteBinary (SaveWriter writer);
        public void ReadBinary (SaveReader reader);

    }

}