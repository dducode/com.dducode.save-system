namespace SaveSystem.Tests.TestObjects {

    public class TestMeshProvider : ISerializationProvider<TestMeshAdapter, TestMesh> {

        public TestMeshAdapter GetAdapter (TestMesh target) {
            return new TestMeshAdapter(target);
        }

    }

}