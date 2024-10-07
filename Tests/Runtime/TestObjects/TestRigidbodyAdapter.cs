using SaveSystemPackage.SerializableData;

namespace SaveSystemPackage.Tests.TestObjects {

    public class TestRigidbodyAdapter {

        public TestRigidbody Target { get; }


        public TestRigidbodyAdapter (TestRigidbody provider) {
            Target = provider;
        }


        public TransformData GetData () {
            return Target.transform;
        }


        public void SetData (TransformData data) {
            Target.position = data.position;
        }

    }

}