using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public sealed class TestMeshAsyncPlayerLoop : MonoBehaviour, IPersistentObjectAsync {

        private MeshFilter m_meshFilter;


        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        public async UniTask Save (UnityWriter writer) {
            writer.Write((MeshData)m_meshFilter.mesh);
            await UniTask.NextFrame();
        }


        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        public async UniTask Load (UnityReader reader) {
            m_meshFilter.mesh = reader.ReadMesh();
            await UniTask.NextFrame();
        }


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
        }


        public void RemoveMesh () {
            m_meshFilter.mesh = null;
        }

    }

}