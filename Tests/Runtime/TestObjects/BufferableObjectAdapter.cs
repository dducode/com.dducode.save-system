using SaveSystem.BinaryHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public class BufferableObjectAdapter : ISerializationAdapter<BufferableObject> {

        public BufferableObject Target { get; }


        public BufferableObjectAdapter (BufferableObject target) {
            Target = target;
        }


        public void Serialize (BinaryWriter writer) {
            var buffer = new DataBuffer();
            buffer.Add(nameof(Target.transform.position), Target.transform.position);
            buffer.Add(nameof(Target.transform.localPosition), Target.transform.localPosition);
            buffer.Add(nameof(Target.transform.rotation), Target.transform.rotation);
            buffer.Add(nameof(Target.transform.localRotation), Target.transform.localRotation);

            buffer.Add(nameof(Target.MeshFilter.mesh), Target.MeshFilter.mesh);
            buffer.Add(nameof(Target.MeshRenderer.material.color), Target.MeshRenderer.material.color);
            writer.Write(buffer);
        }


        public void Deserialize (BinaryReader reader) {
            DataBuffer buffer = reader.ReadDataBuffer();
            Target.transform.rotation = buffer.Get<Quaternion>(nameof(Target.transform.rotation));
            Target.transform.localRotation = buffer.Get<Quaternion>(nameof(Target.transform.localRotation));
            Target.transform.localPosition = buffer.Get<Vector3>(nameof(Target.transform.localPosition));
            Target.transform.position = buffer.Get<Vector3>(nameof(Target.transform.position));

            Target.MeshFilter.mesh = buffer.GetMeshData(nameof(Target.MeshFilter.mesh));
            Target.MeshRenderer.material.color = buffer.Get<Color>(nameof(Target.MeshRenderer.material.color));
        }

    }

}