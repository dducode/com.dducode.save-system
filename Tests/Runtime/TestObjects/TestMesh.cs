using SaveSystem.Attributes;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    [DynamicObject]
    public sealed class TestMesh : MonoBehaviour {

        public MeshData meshData;
        private MeshFilter m_meshFilter;


        public void SetMesh () {
            m_meshFilter.mesh = meshData;
        }


        public void RemoveMesh () {
            m_meshFilter.mesh = null;
            meshData = default;
        }


        private void Awake () {
            m_meshFilter = GetComponent<MeshFilter>();
            meshData = m_meshFilter.mesh;
        }

    }

}