using System.Collections;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.Internal;
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
        public IEnumerator MeshAsyncCancelSave () => UniTask.ToCoroutine(async () => {
            var objects = new TestMeshAsyncThreadPool[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAsyncThreadPool>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");
            await UniTask.NextFrame();

            Debug.Log("Start saving");
            var cancellationSource = new CancellationTokenSource();
            AsyncObjectHandler<TestMeshAsyncThreadPool> objectHandler =
                ObjectHandlersFactory.CreateAsyncHandler(FilePath, objects);

            Debug.Log("Attempt to cancel saving after starting it");
            objectHandler.SaveAsync(cancellationSource.Token).Forget();

            cancellationSource.Cancel();

            Assert.IsTrue(Storage.GetDataSize() == 0);
        });


        [UnityTest]
        public IEnumerator MeshAsyncCancelLoad () => UniTask.ToCoroutine(async () => {
            var objects = new TestMeshAsyncThreadPool[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAsyncThreadPool>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            AsyncObjectHandler<TestMeshAsyncThreadPool> objectHandler =
                ObjectHandlersFactory.CreateAsyncHandler(FilePath, objects);

            HandlingResult result = await objectHandler.SaveAsync();

            if (result == HandlingResult.Success) {
                foreach (TestMeshAsyncThreadPool obj in objects)
                    obj.RemoveMesh();
                Debug.Log("Meshes saved and removed");
            }

            var cancellationSource = new CancellationTokenSource();

            Debug.Log("Attempt to cancel loading before starting it");
            cancellationSource.Cancel();

            Debug.Log("Start loading");
            objectHandler.LoadAsync(cancellationSource.Token).Forget();

            bool condition = objects.Any(obj => obj.MeshIsExists());
            Assert.IsFalse(condition);
        });


        [TearDown]
        public void EndTest () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }

    }

}