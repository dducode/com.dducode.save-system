using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystem.Tests {

    public class CancellationTests {

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
        public IEnumerator MeshAsyncCancelSaveTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            var cancellationSource = new CancellationTokenSource();
            UniTask saveOperation = HandlersProvider
               .CreateObjectHandler(objects, FilePath)
               .OnThreadPool()
               .SetCancellationSource(cancellationSource)
               .OnComplete(_ => {
                    foreach (TestMesh obj in objects)
                        obj.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Meshes saved and removed");
                })
               .SaveAsync();

            Debug.Log("Attempt to cancel save");
            cancellationSource.Cancel();

            await saveOperation;
        });


        [UnityTest]
        public IEnumerator MeshAsyncCancelLoadTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            ObjectHandler objectHandler = HandlersProvider
               .CreateObjectHandler(objects, FilePath)
               .OnThreadPool()
               .OnComplete(_ => {
                    foreach (TestMesh obj in objects)
                        obj.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Meshes saved and removed");
                });
            await objectHandler.SaveAsync();

            Debug.Log("Start loading");
            var cancellationSource = new CancellationTokenSource();
            objectHandler
               .SetCancellationSource(cancellationSource)
               .OnComplete(_ => Debug.Log("Meshes load"));

            Debug.Log("Attempt to cancel load");
            cancellationSource.Cancel();

            await objectHandler.LoadAsync();
        });


        [TearDown]
        public void EndTest () {
            Debug.Log($"Size of data: {DataManager.GetDataSize()} bytes");
            DataManager.DeleteAllData();
            Debug.Log("End test");
        }

    }

}