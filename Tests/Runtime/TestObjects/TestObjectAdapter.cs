using SaveSystemPackage.BinaryHandlers;
using UnityEngine;

namespace SaveSystemPackage.Tests.TestObjects {

    public class TestObjectAdapter : ISerializationAdapter<TestObject> {

        public TestObject Target { get; }


        public TestObjectAdapter (TestObject target) {
            Target = target;
        }


        public void Serialize (SaveWriter writer) {
            var buffer = new DataBuffer();
            buffer.Write(nameof(Target.transform.position), Target.transform.position);
            buffer.Write(nameof(Target.transform.localPosition), Target.transform.localPosition);
            buffer.Write(nameof(Target.transform.rotation), Target.transform.rotation);
            buffer.Write(nameof(Target.transform.localRotation), Target.transform.localRotation);

            writer.Write(Target.MeshFilter.mesh);
            buffer.Write(nameof(Target.MeshRenderer.material.color), Target.MeshRenderer.material.color);
            writer.Write(buffer);
        }


        public void Deserialize (SaveReader reader, int previousVersion) {
            DataBuffer buffer = reader.ReadDataBuffer();
            Target.transform.rotation = buffer.Read<Quaternion>(nameof(Target.transform.rotation));
            Target.transform.localRotation = buffer.Read<Quaternion>(nameof(Target.transform.localRotation));
            Target.transform.localPosition = buffer.Read<Vector3>(nameof(Target.transform.localPosition));
            Target.transform.position = buffer.Read<Vector3>(nameof(Target.transform.position));

            Target.MeshFilter.mesh = reader.ReadMeshData();
            Target.MeshRenderer.material.color = buffer.Read<Color>(nameof(Target.MeshRenderer.material.color));
        }

    }

}