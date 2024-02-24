using System.Collections;
using System.IO;
using NUnit.Framework;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAwaiter = Cysharp.Threading.Tasks.UniTask<SaveSystem.HandlingResult>.Awaiter;

#else
using TaskAwaiter = System.Runtime.CompilerServices.TaskAwaiter<SaveSystem.Handlers.HandlingResult>;
#endif

namespace SaveSystem.Tests {

    public class BufferableObjectsTests {

        private readonly string m_filePath = Path.Combine(Application.persistentDataPath, ".bytes");


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator SerializeObjects () {
            var objectGroup = new DynamicObjectGroup<BufferableObject>(CreateObject, CreateAdapter);

            var bufferableObjects = new BufferableObject[100];

            for (var i = 0; i < 100; i++)
                bufferableObjects[i] = CreateObject();

            objectGroup.AddRange(bufferableObjects);

            using (var writer = new BinaryWriter(File.Open(m_filePath, FileMode.OpenOrCreate)))
                objectGroup.Serialize(writer);

            yield return new WaitForSeconds(2);
        }


        [UnityTest]
        public IEnumerator DeserializeObjects () {
            var objectGroup = new DynamicObjectGroup<BufferableObject>(CreateObject, CreateAdapter);

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