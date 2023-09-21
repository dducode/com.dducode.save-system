using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.InternalServices;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;


namespace SaveSystem.Tests {

    internal sealed class MeshTestsAdvanced {

        private const string FilePath = "test.bytes";
        private const string LpSphereName = "Test LP Sphere";
        private const string HpSphereName = "Test HP Sphere";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator HpMeshAsyncAdvanced () => UniTask.ToCoroutine(async () => {
            TestMeshAdvanced testMono = Object.Instantiate(Resources.Load<TestMeshAdvanced>(HpSphereName));
            Debug.Log("Create object");

            Debug.Log("Start saving");
            AdvancedObjectHandler objectHandler =
                ObjectHandlersFactory.Create(testMono, FilePath);
            await objectHandler.SaveAsync();
            testMono.RemoveMesh();
            Debug.Log("Mesh saved and removed");

            Debug.Log("Start loading");
            await objectHandler.LoadAsync();
            Debug.Log("Mesh loaded");
        });


        [UnityTest]
        public IEnumerator LpMeshesAsyncAdvanced () => UniTask.ToCoroutine(async () => {
            var objects = new TestMeshAdvanced[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAdvanced>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            AdvancedObjectHandler objectHandler = ObjectHandlersFactory.Create(objects, FilePath);
            await objectHandler.SaveAsync();
            foreach (TestMeshAdvanced obj in objects)
                obj.RemoveMesh();
            Debug.Log("Meshes saved and removed");

            Debug.Log("Start loading");
            await objectHandler.LoadAsync();
            Debug.Log("Meshes loaded");
        });


        [TearDown]
        public void EndTest () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }

    }

}