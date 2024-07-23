using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.BinaryHandlers;
using UnityEngine;

namespace SaveSystemPackage.Tests {

    public class BinaryHandlersTests {

        private readonly string m_filePath = Storage.GetFullPath("test.bytes");


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
            const float duration = 1.5f;

            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Debug.Log("Create object");
            await UniTask.WaitForSeconds(duration);

            var meshFilter = gameObject.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;

            await using (var writer = new SaveWriter(File.Open(m_filePath, FileMode.OpenOrCreate)))
                writer.Write(mesh);

            Object.Destroy(meshFilter.mesh);
            Debug.Log("Destroy mesh");
            await UniTask.WaitForSeconds(duration);

            await using (var reader = new SaveReader(File.Open(m_filePath, FileMode.Open)))
                meshFilter.mesh = reader.ReadMeshData();

            Debug.Log("Load mesh");
            await UniTask.WaitForSeconds(duration);
        }


        private static int[] GetData (int itemsCount) {
            var data = new int[itemsCount];
            for (var i = 0; i < data.Length; i++)
                data[i] = i + 1;
            return data;
        }

    }

}