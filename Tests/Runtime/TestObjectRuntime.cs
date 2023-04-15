namespace SaveSystem.Tests.Runtime {

    public class TestObjectRuntime : IPersistentObject {

        public TestMono testMono;
        public string prefabPath;

        public void Save (UnityWriter writer) {
            writer.Write(prefabPath, testMono);
        }


        public void Load (UnityReader reader) {
            testMono = reader.ReadMonoBehaviour<TestMono>();
        }

    }

}