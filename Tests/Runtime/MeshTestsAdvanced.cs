using System.Collections;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;


namespace SaveSystem.Tests.Runtime {

    internal sealed class MeshTestsAdvanced {

        private const string FILE_NAME = "test";
        private const string LP_SPHERE_NAME = "Test LP Sphere";
        private const string HP_SPHERE_NAME = "Test HP Sphere";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator HpMeshAsyncAdvancedTest () => UniTask.ToCoroutine(async () => {
            var testMono = Object.Instantiate(Resources.Load<TestMeshAdvanced>(HP_SPHERE_NAME));
            Debug.Log("Create object");

            Debug.Log("Start saving");
            await DataManager.SaveObjectAsyncAdvanced(
                FILE_NAME,
                testMono,
                null,
                () => {
                    testMono.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Mesh saved and removed");
                }
            );

            Debug.Log("Start loading");
            await DataManager.LoadObjectAsyncAdvanced(
                FILE_NAME,
                testMono,
                null,
                () => Debug.Log("Mesh loaded")
            );
        });


        [UnityTest]
        public IEnumerator LpMeshesAsyncAdvancedTest () => UniTask.ToCoroutine(async () => {
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
            await DataManager.LoadObjectsAsyncAdvanced(
                FILE_NAME,
                objects,
                null,
                null,
                () => Debug.Log("Meshes loaded")
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