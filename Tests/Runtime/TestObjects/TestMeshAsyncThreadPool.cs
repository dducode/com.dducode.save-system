using SaveSystem.UnityHandlers;
using UnityEngine;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAlias = Cysharp.Threading.Tasks.UniTask;
#else
using TaskAlias = System.Threading.Tasks.Task;
#endif

namespace SaveSystem.Tests.TestObjects {

    internal sealed class TestMeshAsyncThreadPool : MonoBehaviour, IPersistentObjectAsync {

        private MeshFilter m_meshFilter;
        private MeshData m_meshData;


        public async TaskAlias Save (UnityWriter writer) {
            await writer.WriteAsync(m_meshData);
        }


        public async TaskAlias Load (UnityReader reader) {
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