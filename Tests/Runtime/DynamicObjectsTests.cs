using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.InternalServices;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SaveSystem.Tests {

    public class DynamicObjectsTests {

        private const string FilePath = "test.bytes";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator SaveLoad () => UniTask.ToCoroutine(async () => {
            ObjectHandler<TestRigidbody> handler = ObjectHandlersFactory.CreateHandler(FilePath, FuncFactory);

            var testObjects = new List<TestRigidbody>();

            Debug.Log("Start objects creation");

            for (var i = 0; i < 100; i++) {
                TestRigidbody testObject = FuncFactory();
                testObject.transform.position = Random.insideUnitSphere * 10;
                testObjects.Add(testObject);
                await UniTask.NextFrame();
            }

            handler.AddObjects(testObjects);
            Debug.Log("Start objects saving");
            handler.Save();

            await UniTask.Delay(500);
            Debug.Log("Start objects destroying");

            foreach (TestRigidbody testObject in testObjects) {
                Object.Destroy(testObject.gameObject);
                await UniTask.NextFrame();
            }

            Debug.Log("Start objects loading");
            handler.Load();

            await UniTask.Delay(500);
            Assert.IsTrue(Object.FindAnyObjectByType<TestRigidbody>());

            TestRigidbody FuncFactory () {
                return GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<TestRigidbody>();
            }
        });


        [TearDown]
        public void EndTest () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }

    }

}