using System.Collections;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystem.Tests.Runtime {

    public class CancellationTests {

        private const string FILE_NAME = "test";
        private const string LP_SPHERE_NAME = "Test LP Sphere";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator MeshAsyncCancelSaveTest () {
            var objects = new TestMeshAsync[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAsync>(LP_SPHERE_NAME));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            var cancellationSource = new CancellationTokenSource();
            var saveOperation = DataManager.SaveObjectsAsync(
                FILE_NAME,
                objects,
                null,
                cancellationSource,
                () => {
                    foreach (var obj in objects)
                        obj.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Meshes saved and removed");
                }
            );

            Debug.Log("Attempt to cancel save");
            cancellationSource.Cancel();

            while (saveOperation.Status == UniTaskStatus.Pending)
                yield return null;
        }


        [UnityTest]
        public IEnumerator MeshAsyncCancelLoadTest () {
            var objects = new TestMeshAsync[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAsync>(LP_SPHERE_NAME));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            var saveOperation = DataManager.SaveObjectsAsync(
                FILE_NAME,
                objects,
                null,
                null,
                () => {
                    foreach (var obj in objects)
                        obj.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Meshes saved and removed");
                }
            );


            while (saveOperation.Status == UniTaskStatus.Pending)
                yield return null;

            Debug.Log("Start loading");
            var cancellationSource = new CancellationTokenSource();
            var loadOperation = DataManager.LoadObjectsAsync(
                FILE_NAME,
                objects,
                null,
                cancellationSource,
                () => Debug.Log("Meshes load")
            );

            Debug.Log("Attempt to cancel load");
            cancellationSource.Cancel();

            while (loadOperation.Status == UniTaskStatus.Pending)
                yield return null;
        }


        [TearDown]
        public void EndTest () {
            var method = typeof(DataManager).GetMethod("GetDataSize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });

            method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
            Debug.Log("End test");
        }

    }

}