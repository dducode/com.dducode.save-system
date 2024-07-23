using SaveSystemPackage.BinaryHandlers;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [RequireComponent(typeof(MeshFilter))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Save System/Mesh Filter Recorder")]
    public class MeshFilterRecorder : MonoBehaviour, ISerializationAdapter<MeshFilter> {

        public MeshFilter Target { get; private set; }


        private void Awake () {
            Target = GetComponent<MeshFilter>();
        }


        public void Serialize (SaveWriter writer) {
            writer.Write(Target.mesh);
        }


        public void Deserialize (SaveReader reader, int previousVersion) {
            Target.mesh = reader.ReadMeshData();
        }

    }

}