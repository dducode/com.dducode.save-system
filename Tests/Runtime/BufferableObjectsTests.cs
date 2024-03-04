using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.BinaryHandlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;


namespace SaveSystem.Tests {

    public class BufferableObjectsTests {

        private readonly string m_filePath = Storage.GetFullPath(".bytes");


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [Test]
        public async Task SerializeObjects () {
            var objectGroup = new DynamicObjectGroup<BufferableObject>(
                new BufferableObjectFactory(), new BufferableObjectProvider()
            );
            objectGroup.CreateObjects(100);

            using (var writer = new SaveWriter(File.Open(m_filePath, FileMode.OpenOrCreate)))
                await objectGroup.Serialize(writer, CancellationToken.None);

            await UniTask.WaitForSeconds(2);
        }


        [Test]
        public async Task DeserializeObjects () {
            var objectGroup =
                new DynamicObjectGroup<BufferableObject>(
                    new BufferableObjectFactory(), new BufferableObjectProvider()
                );

            using (var reader = new SaveReader(File.Open(m_filePath, FileMode.Open)))
                await objectGroup.Deserialize(reader, CancellationToken.None);

            await UniTask.WaitForSeconds(2);
        }


        [TearDown]
        public void End () {
            Debug.Log("End test");
        }

    }

}