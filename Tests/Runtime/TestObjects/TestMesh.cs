using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    internal sealed class TestMesh : MonoBehaviour, IPersistentObject {

        private MeshFilter m_meshFilter;
        private MeshData m_meshData;


        public void Save (UnityWriter writer) {
            writer.Write(m_meshData);
        }


        public void Load (UnityReader reader) {
            m_meshData = reader.ReadMesh();
        }


        public void RemoveMesh () {
            m_meshFilter.mesh = null;
        }


        public void SetMesh () {
            m_meshFilter.mesh = m_meshData;
        }


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
            m_meshData = m_meshFilter.mesh;
        }

    }

}