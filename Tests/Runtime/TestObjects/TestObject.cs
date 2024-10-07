using UnityEngine;

namespace SaveSystemPackage.Tests.TestObjects {

    public class TestObject : MonoBehaviour {

        public MeshFilter MeshFilter { get; private set; }
        public MeshRenderer MeshRenderer { get; private set; }


        private void Awake () {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();
        }


        public TestObject SetRandomTransform () {
            transform.position = Random.insideUnitSphere * 10;
            transform.rotation = Random.rotation;

            return this;
        }


        public TestObject SetRandomColor () {
            MeshRenderer.material.color = Random.ColorHSV(
                0.5f, 1f, 0.75f, 1f, 0, 1
            );

            return this;
        }


        public void Reset () {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            MeshRenderer.material.color = MeshRenderer.sharedMaterial.color;
        }

    }

}