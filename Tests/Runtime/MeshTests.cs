using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystem.Tests {

    internal sealed class MeshTests {

        private const string FilePath = "test.bytes";
        private const string LpSphereName = "Test LP Sphere";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator MeshTest () {
            TestMesh testMono = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
            Debug.Log("Create object");
            yield return new WaitForSeconds(2);

            ObjectHandler objectHandler = HandlersProvider.CreateObjectHandler(testMono, FilePath);
            objectHandler.Save();
            testMono.GetComponent<MeshFilter>().mesh = null;
            Debug.Log("Save and remove mesh");
            yield return new WaitForSeconds(2);

            objectHandler.Load();
            Debug.Log("Load mesh");
            yield return new WaitForSeconds(2);
        }


        [UnityTest]
        public IEnumerator MeshesAsyncOnThreadPoolTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            ObjectHandler objectHandler = HandlersProvider.CreateObjectHandler(objects, FilePath);
            Debug.Log("Start saving");
            await objectHandler
               .OnThreadPool()
               .OnComplete(_ => {
                    foreach (TestMesh obj in objects)
                        obj.RemoveMesh();
                    Debug.Log("Meshes saved and removed");
                })
               .SaveAsync();

            Debug.Log("Start loading");
            await objectHandler
               .OnComplete(_ => {
                    foreach (TestMesh obj in objects)
                        obj.SetMesh();
                    Debug.Log("Meshes loaded");
                })
               .LoadAsync();
        });


        [UnityTest]
        public IEnumerator MeshesAsyncOnPlayerLoopTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            ObjectHandler objectHandler = HandlersProvider
               .CreateObjectHandler(objects, FilePath)
               .OnPlayerLoop()
               .OnComplete(_ => {
                    foreach (TestMesh obj in objects)
                        obj.RemoveMesh();
                    Debug.Log("Meshes saved and removed");
                });
            await objectHandler.SaveAsync();

            Debug.Log("Start loading");
            await objectHandler
               .OnComplete(_ => {
                    foreach (TestMesh obj in objects)
                        obj.SetMesh();
                    Debug.Log("Meshes loaded");
                })
               .LoadAsync();
        });


        [TearDown]
        public void EndTest () {
            DataManager.DeleteAllData();
            Debug.Log("End test");
        }

    }

}