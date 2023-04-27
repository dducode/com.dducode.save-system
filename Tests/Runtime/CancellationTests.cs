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
        public IEnumerator MeshAsyncCancelSaveTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMeshAdvanced[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAdvanced>(LP_SPHERE_NAME));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            var cancellationSource = new CancellationTokenSource();
            var saveOperation = DataManager.SaveObjectsAsyncAdvanced(
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

            await saveOperation;
        });


        [UnityTest]
        public IEnumerator MeshAsyncCancelLoadTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMeshAdvanced[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshAdvanced>(LP_SPHERE_NAME));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            await DataManager.SaveObjectsAsyncAdvanced(
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

            Debug.Log("Start loading");
            var cancellationSource = new CancellationTokenSource();
            var loadOperation = DataManager.LoadObjectsAsyncAdvanced(
                FILE_NAME,
                objects,
                null,
                cancellationSource,
                () => Debug.Log("Meshes load")
            );

            Debug.Log("Attempt to cancel load");
            cancellationSource.Cancel();

            await loadOperation;
        });


        [TearDown]
        public void EndTest () {
            var method = typeof(SaveSystemEditor).GetMethod("GetDataSize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });

            method = typeof(SaveSystemEditor).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
            Debug.Log("End test");
        }

    }

}