using SaveSystem.BinaryHandlers;

namespace SaveSystem.Tests.TestObjects {

    public class TestMeshAdapter : ISerializationAdapter<TestMesh> {

        public TestMesh Target { get; }


        public TestMeshAdapter (TestMesh provider) {
            Target = provider;
        }


        public void Serialize (BinaryWriter writer) {
            writer.Write(Target.meshData);
        }


        public void Deserialize (BinaryReader reader) {
            Target.meshData = reader.ReadMeshData();
        }

    }

}