using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [RequireComponent(typeof(MeshFilter))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Save System/Mesh Filter Recorder")]
    public class MeshFilterRecorder : ComponentRecorder {

        private SerializationScope m_scope;

        public MeshFilter Target { get; private set; }


        public override async Task Initialize (SerializationScope scope) {
            m_scope = scope;
            Target = GetComponent<MeshFilter>();
            Target.mesh = await m_scope.LoadData<MeshData>(Id, SaveSystem.exitCancellation.Token);
            m_scope.OnSave += async _ => {
                await m_scope.SaveData(Id, (MeshData)Target.mesh, SaveSystem.exitCancellation.Token);
            };
        }


        public override string ToString () {
            return $"{gameObject.name} Mesh Filter Recorder";
        }

    }

}