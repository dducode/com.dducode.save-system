using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    internal sealed class TestMeshAdvanced : MonoBehaviour, IPersistentObjectAsync {

        private MeshFilter m_meshFilter;


        public async UniTask Save (UnityAsyncWriter asyncWriter) {
            await asyncWriter.Write(m_meshFilter.mesh);
        }


        public async UniTask Load (UnityAsyncReader asyncReader) {
            m_meshFilter.mesh = await asyncReader.ReadMesh();
        }


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
        }

    }

}