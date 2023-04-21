using System.Collections;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;


namespace SaveSystem.Tests.Runtime {

    internal sealed class MeshTests {

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
        public IEnumerator HPMeshAsyncOnThreadPoolTest () {
            var testMono = Object.Instantiate(Resources.Load<TestMeshAsync>(HP_SPHERE_NAME));
            Debug.Log("Create object");

            Debug.Log("Start saving");
            var saveOperation = DataManager.SaveObjectAsync(
                FILE_NAME,
                testMono,
                null,
                () => {
                    testMono.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Mesh saved and removed");
                });

            while (saveOperation.Status == UniTaskStatus.Pending)
                yield return null;

            Debug.Log("Start loading");
            var loadOperation = DataManager.LoadObjectAsync(
                FILE_NAME,
                testMono,
                null,
                () => Debug.Log("Mesh loaded")
            );

            while (loadOperation.Status == UniTaskStatus.Pending)
                yield return null;
        }


        [UnityTest]
        public IEnumerator HPMeshAsyncOnPlayerLoopTest () {
            var testMono = Object.Instantiate(Resources.Load<TestMeshUnityAsync>(HP_SPHERE_NAME));
            Debug.Log("Create object");

            Debug.Log("Start saving");
            var saveOperation = DataManager.SaveObjectAsync(
                FILE_NAME,
                testMono,
                null,
                () => {
                    testMono.GetComponent<MeshFilter>().mesh = null;
                    Debug.Log("Mesh saved and removed");
                });

            while (saveOperation.Status == UniTaskStatus.Pending)
                yield return null;

            Debug.Log("Start loading");
            var loadOperation = DataManager.LoadObjectAsync(
                FILE_NAME,
                testMono,
                null,
                () => Debug.Log("Mesh loaded")
            );

            while (loadOperation.Status == UniTaskStatus.Pending)
                yield return null;
        }


        [UnityTest]
        public IEnumerator LPMeshesAsyncOnThreadPoolTest () {
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
            var loadOperation = DataManager.LoadObjectsAsync(
                FILE_NAME,
                objects,
                null,
                null,
                () => Debug.Log("Meshes loaded")
            );

            while (loadOperation.Status == UniTaskStatus.Pending)
                yield return null;
        }


        [UnityTest]
        public IEnumerator LPMeshesAsyncOnPlayerLoopTest () {
            var objects = new TestMeshUnityAsync[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMeshUnityAsync>(LP_SPHERE_NAME));
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
            var loadOperation = DataManager.LoadObjectsAsync(
                FILE_NAME,
                objects,
                null,
                null,
                () => Debug.Log("Meshes loaded")
            );

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