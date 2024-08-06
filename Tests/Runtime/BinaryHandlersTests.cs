using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.Serialization;
using UnityEngine;

namespace SaveSystemPackage.Tests {

    public class BinaryHandlersTests {

        private readonly string m_filePath = Path.Combine(Storage.Root.Path, "test.bytes");


        [SetUp]
        public void Start () {
            new GameObject("camera") {
                transform = {
                    position = new Vector3(0, 0, -10)
                }
            }.AddComponent<Camera>();
        }


        [Test]
        public void WriteRead () {
            using (var writer = new SaveWriter(File.Open(m_filePath, FileMode.OpenOrCreate)))
                writer.Write(GetData(25));

            using (var reader = new SaveReader(File.Open(m_filePath, FileMode.Open))) {
                var message = new StringBuilder();
                int[] data = reader.ReadArray<int>();
                foreach (int i in data)
                    message.Append($"item: {i}\n");
                Debug.Log(message);
            }
        }


        [Test]
        public async Task WriteReadMesh () {
            const int duration = 1500;

            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Debug.Log("Create object");
            await Task.Delay(duration);

            var meshFilter = gameObject.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;

            await using (var writer = new SaveWriter(File.Open(m_filePath, FileMode.OpenOrCreate)))
                writer.Write(mesh);

            Object.Destroy(meshFilter.mesh);
            Debug.Log("Destroy mesh");
            await Task.Delay(duration);

            await using (var reader = new SaveReader(File.Open(m_filePath, FileMode.Open)))
                meshFilter.mesh = reader.ReadMeshData();

            Debug.Log("Load mesh");
            await Task.Delay(duration);
        }


        private static int[] GetData (int itemsCount) {
            var data = new int[itemsCount];
            for (var i = 0; i < data.Length; i++)
                data[i] = i + 1;
            return data;
        }

    }

}