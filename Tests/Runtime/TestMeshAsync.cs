using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystem.Tests.Runtime {

    public class TestMeshAsync : MonoBehaviour, IPersistentObjectAsync {

        private MeshFilter m_meshFilter;


        public async Task Save (UnityWriter writer) {
            await writer.WriteAsync(m_meshFilter.mesh);
        }


        public async Task Load (UnityReader reader) {
            m_meshFilter.mesh = await reader.ReadMeshAsync();
        }


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
        }

    }

}