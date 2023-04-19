using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;


namespace SaveSystem.Tests.Runtime {

    public class SaveSystemRuntimeTests {

        private const string FILE_NAME = "test";
        private const string CUBE_NAME = "Test Cube";
        private const string SPHERE_NAME = "Test Sphere";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator MeshTest () {
            var testMono = Object.Instantiate(Resources.Load<TestMesh>(CUBE_NAME));
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
        public IEnumerator MeshAsyncTest () {
            var testMono = Object.Instantiate(Resources.Load<TestMeshAsync>(SPHERE_NAME));
            Debug.Log("Create object");
            yield return new WaitForSeconds(2);

            var saveOperation = DataManager.SaveObjectsAsync(
                FILE_NAME,
                new[] {testMono},
                null,
                null,
                () => Debug.Log("Mesh saved")
            );
            testMono.GetComponent<MeshFilter>().mesh = null;
            Debug.Log("Saving and removing a mesh");

            while (!saveOperation.IsCompleted)
                yield return null;

            var loadOperation = DataManager.LoadObjectsAsync(
                FILE_NAME,
                new[] {testMono},
                null,
                null,
                () => Debug.Log("Mesh loaded")
            );
            Debug.Log("Loading the mesh");

            while (!loadOperation.IsCompleted)
                yield return null;

            yield return new WaitForSeconds(2);
        }


        [UnityTest]
        public IEnumerator MeshCoroutineTest () {
            var testMono = Object.Instantiate(Resources.Load<TestMesh>(CUBE_NAME));
            Debug.Log("Create object");
            yield return new WaitForSeconds(2);

            Debug.Log("Saving and removing a mesh");
            yield return DataManager.SaveObjectsCoroutine(
                FILE_NAME,
                new[] {testMono},
                null,
                () => Debug.Log("Mesh saved")
            );
            testMono.GetComponent<MeshFilter>().mesh = null;

            Debug.Log("Loading the mesh");
            yield return DataManager.LoadObjectsCoroutine(
                FILE_NAME,
                new[] {testMono},
                success => {
                    if (success)
                        Debug.Log("Mesh loaded");
                }
            );

            yield return new WaitForSeconds(2);
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