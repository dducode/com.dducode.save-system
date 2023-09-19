using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
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
        public IEnumerator HpMeshAsyncAdvancedTest () => UniTask.ToCoroutine(async () => {
            TestMeshAdvanced testMono = Object.Instantiate(Resources.Load<TestMeshAdvanced>(HpSphereName));
            Debug.Log("Create object");

            Debug.Log("Start saving");
            AdvancedObjectHandler objectHandler =
                HandlersProvider.CreateObjectHandler(testMono, FilePath);
            await objectHandler
               .OnComplete(_ => {
                    testMono.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Mesh saved and removed");
                })
               .SaveAsync();

            Debug.Log("Start loading");
            await objectHandler
               .OnComplete(_ => Debug.Log("Mesh loaded"))
               .LoadAsync();
        });


        [UnityTest]
        public IEnumerator LpMeshesAsyncAdvancedTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMeshAdvanced[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAdvanced>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            AdvancedObjectHandler objectHandler =
                HandlersProvider.CreateObjectHandler(objects, FilePath);
            await objectHandler
               .OnComplete(_ => {
                    foreach (TestMeshAdvanced obj in objects)
                        obj.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Meshes saved and removed");
                })
               .SaveAsync();

            Debug.Log("Start loading");
            await objectHandler
               .OnComplete(_ => Debug.Log("Meshes loaded"))
               .LoadAsync();
        });


        [TearDown]
        public void EndTest () {
            DataManager.DeleteAllData();
            Debug.Log("End test");
        }

    }

}