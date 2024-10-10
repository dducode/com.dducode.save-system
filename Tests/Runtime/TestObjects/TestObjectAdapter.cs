namespace SaveSystemPackage.Tests.TestObjects {

    public class TestObjectAdapter {

        public TestObject Target { get; }


        public TestObjectAdapter (TestObject target) {
            Target = target;
        }


        public TestObjectData GetData () {
            return new TestObjectData {
                transformData = Target.transform,
                color = Target.MeshRenderer.material.color,
                meshData = Target.MeshFilter.mesh
            };
        }


        public void SetData (TestObjectData data) {
            Target.MeshFilter.mesh = data.meshData;
            Target.transform.rotation = data.transformData.rotation;
            Target.transform.localRotation = data.transformData.localRotation;
            Target.transform.localPosition = data.transformData.localPosition;
            Target.transform.position = data.transformData.position;
            Target.MeshRenderer.material.color = data.color;
        }

    }

}