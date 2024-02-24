using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public class ComponentProvider : MonoBehaviour {

        [SerializeField]
        private TestMesh testMesh;

        [SerializeField]
        private Rigidbody rb;

        public TestMesh TestMesh => testMesh;
        public Transform Transform => transform;
        public Rigidbody Rigidbody => rb;

    }

}