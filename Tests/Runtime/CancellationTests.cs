using System.Collections;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAwaiter = Cysharp.Threading.Tasks.UniTask<SaveSystem.Handlers.HandlingResult>.Awaiter;
#else
using TaskAwaiter = System.Runtime.CompilerServices.TaskAwaiter<SaveSystem.Handlers.HandlingResult>;
#endif

#pragma warning disable CS4014

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
        public IEnumerator MeshAsyncCancelSave () {
            var objects = new TestMeshAsyncThreadPool[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAsyncThreadPool>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");
            yield return new WaitForEndOfFrame();

            Debug.Log("Start saving");
            var cancellationSource = new CancellationTokenSource();
            AsyncObjectHandler<TestMeshAsyncThreadPool> objectHandler =
                ObjectHandlersFactory.CreateAsyncHandler(FilePath, objects);

            Debug.Log("Attempt to cancel saving after starting it");
            objectHandler.SaveAsync(cancellationSource.Token);

            cancellationSource.Cancel();

            Assert.IsTrue(Storage.GetDataSize() == 0);
        }


        [UnityTest]
        public IEnumerator MeshAsyncCancelLoad () {
            var objects = new TestMeshAsyncThreadPool[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAsyncThreadPool>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            AsyncObjectHandler<TestMeshAsyncThreadPool> objectHandler =
                ObjectHandlersFactory.CreateAsyncHandler(FilePath, objects);

            TaskAwaiter task = objectHandler.SaveAsync().GetAwaiter();
            var awaiter = new WaitWhile(() => !task.IsCompleted);
            yield return awaiter;

            if (task.GetResult() == HandlingResult.Success) {
                foreach (TestMeshAsyncThreadPool obj in objects)
                    obj.RemoveMesh();
                Debug.Log("Meshes saved and removed");
            }

            var cancellationSource = new CancellationTokenSource();

            Debug.Log("Attempt to cancel loading before starting it");
            cancellationSource.Cancel();

            Debug.Log("Start loading");
            objectHandler.LoadAsync(cancellationSource.Token);

            bool condition = objects.Any(obj => obj.MeshIsExists());
            Assert.IsFalse(condition);
        }


        [TearDown]
        public void EndTest () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }

    }

}