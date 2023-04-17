using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;


namespace SaveSystem.Tests.Runtime {

    public class SaveSystemRuntimeTests {

        private const string FILE_NAME = "test";
        private const string PREFAB_NAME = "Test Prefab";


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

            var testMono = Object.Instantiate(Resources.Load<TestMesh>(PREFAB_NAME));
            Debug.Log("Create object");
            yield return new WaitForSeconds(2);

            DataManager.SaveObject(FILE_NAME, testMono);
            testMono.GetComponent<MeshFilter>().mesh = null;
            Debug.Log("Save and remove mesh");
            yield return new WaitForSeconds(2);

            DataManager.LoadObject(FILE_NAME, testMono);
            Debug.Log("Load mesh");
            yield return new WaitForSeconds(2);

            var method = typeof(DataManager).GetMethod("GetDataSize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });

            method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
            Debug.Log("End test");
        }


        [UnityTest]
        public IEnumerator AsyncTest () {
            var testObject = new TestObjectRuntime {
                name = "Test Object",
                intValue = 12345,
                boolValue = true
            };

            var objects = new List<TestObjectRuntime>();

            for (var i = 0; i < 2_500; i++)
                objects.Add(testObject);

            var showing = new Showing();
            var asyncOperation = DataManager.SaveObjectsAsync(FILE_NAME, objects.ToArray(), showing);

            while (!asyncOperation.IsCompleted)
                yield return null;
        }


        [UnityTest]
        public IEnumerator CancelAsyncTest () {
            var testObject = new TestObjectRuntime {
                name = "Test Object",
                intValue = 12345,
                boolValue = true
            };

            var objects = new List<TestObjectRuntime>();

            for (var i = 0; i < 2_500; i++)
                objects.Add(testObject);

            var source = new CancellationTokenSource();
            var showing = new Showing();
            var asyncOperation = DataManager.SaveObjectsAsync(
                FILE_NAME, objects.ToArray(), showing, source);
            var iterations = 0;

            while (!asyncOperation.IsCompleted) {
                iterations++;
                if (iterations >= 1_000)
                    source.Cancel();
                yield return null;
            }
        }

    }

}