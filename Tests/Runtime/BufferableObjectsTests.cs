using System.Collections;
using System.IO;
using NUnit.Framework;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;


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


        [UnityTest]
        public IEnumerator SerializeObjects () {
            var objectGroup = new DynamicObjectFactory<BufferableObject>(CreateObject, CreateAdapter);
            objectGroup.CreateObjects(100);

            using (var writer = new BinaryWriter(File.Open(m_filePath, FileMode.OpenOrCreate)))
                objectGroup.Serialize(writer);

            yield return new WaitForSeconds(2);
        }


        [UnityTest]
        public IEnumerator DeserializeObjects () {
            var objectGroup = new DynamicObjectFactory<BufferableObject>(CreateObject, CreateAdapter);

            using (var reader = new BinaryReader(File.Open(m_filePath, FileMode.Open)))
                objectGroup.Deserialize(reader);

            yield return new WaitForSeconds(2);
        }


        [TearDown]
        public void End () {
            Debug.Log("End test");
        }


        private BufferableObjectAdapter CreateAdapter (BufferableObject obj) {
            return new BufferableObjectAdapter(obj);
        }


        private BufferableObject CreateObject () {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.AddComponent<BufferableObject>().SetRandomTransform().SetRandomColor();
            return gameObject.AddComponent<BufferableObject>();
        }

    }

}