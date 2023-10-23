using System.Diagnostics.CodeAnalysis;
using SaveSystem.UnityHandlers;
using UnityEngine;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SaveSystem.Tests.TestObjects {

    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
    public sealed class TestMeshAsyncPlayerLoop : MonoBehaviour, IPersistentObjectAsync {

        private MeshFilter m_meshFilter;


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask Save (UnityWriter writer) {
            writer.Write((MeshData)m_meshFilter.mesh);
            await UniTask.NextFrame();
        }


        public async UniTask Load (UnityReader reader) {
            m_meshFilter.mesh = reader.ReadMesh();
            await UniTask.NextFrame();
        }
    #else
        public async Task Save (UnityWriter writer) {
            writer.Write((MeshData)m_meshFilter.mesh);
            await Task.Delay(1);
        }


        public async Task Load (UnityReader reader) {
            m_meshFilter.mesh = reader.ReadMesh();
            await Task.Delay(1);
        }
    #endif


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
        }


        public void RemoveMesh () {
            m_meshFilter.mesh = null;
        }

    }

}