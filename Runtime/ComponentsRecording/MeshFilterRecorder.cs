using SaveSystemPackage.BinaryHandlers;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [RequireComponent(typeof(MeshFilter))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Save System/Mesh Filter Recorder")]
    public class MeshFilterRecorder : ComponentRecorder, ISerializationAdapter<MeshFilter> {

        public MeshFilter Target { get; private set; }


        private void Awake () {
            Target = GetComponent<MeshFilter>();
        }


        public override void Serialize (SaveWriter writer) {
            writer.Write(Target.mesh);
        }


        public override void Deserialize (SaveReader reader, int previousVersion) {
            Target.mesh = reader.ReadMeshData();
        }


        public override string ToString () {
            return $"{gameObject.name} Mesh Filter Recorder";
        }

    }

}