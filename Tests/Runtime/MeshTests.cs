using System.Collections;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystem.Tests.Runtime {

    public class MeshTests {

        private const string FILE_NAME = "test";
        private const string SETTINGS = "test settings";
        private const string LP_SPHERE_NAME = "Test LP Sphere";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator MeshTest () {
            var testMono = Object.Instantiate(Resources.Load<TestMesh>(LP_SPHERE_NAME));
            Debug.Log("Create object");
            yield return new WaitForSeconds(2);

            DataManager.SaveObject(FILE_NAME, testMono);
            testMono.GetComponent<MeshFilter>().mesh = null;
            Debug.Log("Save and remove mesh");
            yield return new WaitForSeconds(2);

            DataManager.LoadObject(FILE_NAME, testMono);
            Debug.Log("Load mesh");
            yield return new WaitForSeconds(2);
        }


        [UnityTest]
        public IEnumerator MeshesAsyncOnThreadPoolTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LP_SPHERE_NAME));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            await DataManager.SaveObjectsAsync(
                FILE_NAME,
                objects,
                AsyncMode.OnThreadPool,
                null,
                null,
                () => {
                    foreach (var obj in objects)
                        obj.RemoveMesh();
                    Debug.Log("Meshes saved and removed");
                }
            );

            Debug.Log("Start loading");
            await DataManager.LoadObjectsAsync(
                FILE_NAME,
                objects,
                AsyncMode.OnThreadPool,
                null,
                null,
                () => {
                    foreach (var obj in objects)
                        obj.SetMesh();
                    Debug.Log("Meshes loaded");
                }
            );
        });


        [UnityTest]
        public IEnumerator MeshesAsyncOnPlayerLoopTest () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LP_SPHERE_NAME));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            await DataManager.SaveObjectsAsync(
                FILE_NAME,
                objects,
                AsyncMode.OnPlayerLoop,
                null,
                null,
                () => {
                    foreach (var obj in objects)
                        obj.RemoveMesh();
                    Debug.Log("Meshes saved and removed");
                }
            );

            Debug.Log("Start loading");
            await DataManager.LoadObjectsAsync(
                FILE_NAME,
                objects,
                AsyncMode.OnPlayerLoop,
                null,
                null,
                () => {
                    foreach (var obj in objects)
                        obj.SetMesh();
                    Debug.Log("Meshes loaded");
                }
            );
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