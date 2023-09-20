using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    internal sealed class TestMeshAdvanced : MonoBehaviour, IPersistentObjectAsync {

        private MeshFilter m_meshFilter;


        public async UniTask Save (UnityWriter writer) {
            await writer.WriteAsync(m_meshFilter.mesh);
        }


        public async UniTask Load (UnityReader reader) {
            m_meshFilter.mesh = await reader.ReadMeshAsync();
        }


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
        }


        public void RemoveMesh () {
            m_meshFilter.mesh = null;
        }

    }

}