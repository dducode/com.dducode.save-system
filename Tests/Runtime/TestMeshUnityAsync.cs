using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SaveSystem.Tests.Runtime {

    internal sealed class TestMeshUnityAsync : MonoBehaviour, IPersistentObjectAsync {

        private MeshFilter m_meshFilter;


        public async UniTask Save (UnityWriter writer) {
            await writer.WriteAsync(m_meshFilter.mesh, AsyncMode.OnPlayerLoop);
        }


        public async UniTask Load (UnityReader reader) {
            m_meshFilter.mesh = await reader.ReadMeshAsync(AsyncMode.OnPlayerLoop);
        }


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
        }

    }

}