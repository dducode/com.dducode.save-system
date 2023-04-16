using UnityEngine;

namespace SaveSystem.Tests.Runtime {

    public class TestMesh : MonoBehaviour, IPersistentObject {

        public void Save (UnityWriter writer) {
            writer.Write(GetComponent<MeshFilter>().mesh);
        }


        public void Load (UnityReader reader) {
            GetComponent<MeshFilter>().mesh = reader.ReadMesh();
        }

    }

}