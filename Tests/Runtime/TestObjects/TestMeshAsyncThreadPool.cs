using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    internal sealed class TestMeshAsyncThreadPool : MonoBehaviour, IPersistentObjectAsync {

        private MeshFilter m_meshFilter;
        private MeshData m_meshData;


        public async UniTask Save (UnityWriter writer) {
            await writer.WriteAsync(m_meshFilter.mesh);
        }


        public async UniTask Load (UnityReader reader) {
            m_meshData = await reader.ReadMeshAsync();
            m_meshFilter.mesh = m_meshData;
        }


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
            m_meshData = m_meshFilter.mesh;
        }


        public void RemoveMesh () {
            m_meshFilter.mesh = null;
            m_meshData = default;
        }


        public bool MeshIsExists () {
            return m_meshData != default;
        }

    }

}