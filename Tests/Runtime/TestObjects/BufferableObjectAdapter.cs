using SaveSystem.BinaryHandlers;

namespace SaveSystem.Tests.TestObjects {

    public class BufferableObjectAdapter : ISerializationAdapter<BufferableObject> {

        public BufferableObject Target { get; }
        public bool DontDestroyOnSceneUnload { get; }


        public BufferableObjectAdapter (BufferableObject target) {
            Target = target;
            DontDestroyOnSceneUnload = false;
        }


        public void Serialize (BinaryWriter writer) {
            var buffer = new DataBuffer();
            buffer.Add(Target.transform.position, nameof(Target.transform.position));
            buffer.Add(Target.transform.localPosition, nameof(Target.transform.localPosition));
            buffer.Add(Target.transform.rotation, nameof(Target.transform.rotation));
            buffer.Add(Target.transform.localRotation, nameof(Target.transform.localRotation));

            buffer.Add((MeshData)Target.MeshFilter.mesh);
            buffer.Add(Target.MeshRenderer.material.color);
            writer.Write(buffer);
        }


        public void Deserialize (BinaryReader reader) {
            DataBuffer buffer = reader.ReadDataBuffer();
            Target.transform.rotation = buffer.GetQuaternion(nameof(Target.transform.rotation));
            Target.transform.localRotation = buffer.GetQuaternion(nameof(Target.transform.localRotation));
            Target.transform.localPosition = buffer.GetVector3(nameof(Target.transform.localPosition));
            Target.transform.position = buffer.GetVector3(nameof(Target.transform.position));

            Target.MeshFilter.mesh = buffer.GetMeshData();
            Target.MeshRenderer.material.color = buffer.GetColor();
        }

    }

}