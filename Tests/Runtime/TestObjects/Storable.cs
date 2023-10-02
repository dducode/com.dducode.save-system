using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public class Storable : MonoBehaviour, IStorable {

        private MeshFilter m_filter;
        private MeshRenderer m_renderer;


        private void Awake () {
            m_filter = GetComponent<MeshFilter>();
            m_renderer = GetComponent<MeshRenderer>();
        }


        public Storable SetRandomTransform () {
            transform.position = Random.insideUnitSphere * 10;
            transform.rotation = Random.rotation;

            return this;
        }


        public Storable SetRandomColor () {
            m_renderer.material.color = Random.ColorHSV(
                0.5f, 1f, 0.75f, 1f, 0, 1
            );

            return this;
        }


        public void Reset () {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            m_renderer.material.color = m_renderer.sharedMaterial.color;
        }


        public DataBuffer Save () {
            return new DataBuffer {
                vector3 = transform.position,
                quaternion = transform.rotation,
                color = m_renderer.material.color,
                meshData = m_filter.mesh
            };
        }


        public void Load (DataBuffer buffer) {
            transform.rotation = buffer.quaternion;
            m_renderer.material.color = buffer.color;
            transform.position = buffer.vector3;
            m_filter.mesh = buffer.meshData;
        }

    }

}