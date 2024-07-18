using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.BinaryHandlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using Object = UnityEngine.Object;


namespace SaveSystem.Tests {

    public class TestObjectsTests {

        private readonly string m_filePath = Path.Combine(Application.temporaryCachePath, "test-objects.data");


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [Test, Order(0)]
        public async Task SerializeObjects () {
            var objectGroup = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Cube), new TestObjectProvider()
            );
            objectGroup.CreateObjects(100);

            await using (var writer = new SaveWriter(File.Open(m_filePath, FileMode.OpenOrCreate)))
                objectGroup.Serialize(writer);

            await UniTask.WaitForSeconds(1);
            objectGroup.DoForAll(obj => Object.Destroy(obj.gameObject));
            await UniTask.WaitForSeconds(0.2f);
        }


        [Test, Order(1)]
        public async Task DeserializeObjects () {
            var objectGroup = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Cube), new TestObjectProvider()
            );

            await using (var reader = new SaveReader(File.Open(m_filePath, FileMode.Open)))
                objectGroup.Deserialize(reader, 0);

            await UniTask.WaitForSeconds(1);
        }


        [TearDown]
        public void End () {
            Debug.Log("End test");
        }

    }

}