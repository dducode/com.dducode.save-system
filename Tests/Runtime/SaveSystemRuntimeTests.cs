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
            yield return new WaitForSeconds(2);

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

            var saveOperation = DataManager.SaveObjectsAsync(FILE_NAME, new[] {testMono});
            testMono.GetComponent<MeshFilter>().mesh = null;
            Debug.Log("Save and remove mesh");

            while (!saveOperation.IsCompleted)
                yield return null;

            var loadOperation = DataManager.LoadObjectsAsync(FILE_NAME, new[] {testMono});
            Debug.Log("Load mesh");

            while (!loadOperation.IsCompleted)
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