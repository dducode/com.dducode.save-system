namespace SaveSystem.Tests.Runtime {

    public class TestObjectRuntime : IPersistentObject {

        public string name;
        public int intValue;
        public bool boolValue;


        public void Save (UnityWriter writer) {
            writer.Write(name);
            writer.Write(intValue);
            writer.Write(boolValue);
        }


        public void Load (UnityReader reader) {
            name = reader.ReadString();
            intValue = reader.ReadInt();
            boolValue = reader.ReadBool();
        }

    }

}