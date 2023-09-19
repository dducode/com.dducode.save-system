using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    [RequireComponent(typeof(Rigidbody))]
    public class TestRigidbody : MonoBehaviour, IPersistentObject {

        private Vector3 m_position;


        private void FixedUpdate () {
            m_position = transform.position;
        }


        public void Save (UnityWriter writer) {
            writer.Write(m_position);
        }


        public void Load (UnityReader reader) {
            transform.position = reader.ReadVector3();
        }

    }

}