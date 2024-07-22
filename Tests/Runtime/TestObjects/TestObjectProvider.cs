namespace SaveSystemPackage.Tests.TestObjects {

    public class TestObjectProvider : ISerializationProvider<TestObjectAdapter, TestObject> {

        public TestObjectAdapter GetAdapter (TestObject target) {
            return new TestObjectAdapter(target);
        }

    }

}