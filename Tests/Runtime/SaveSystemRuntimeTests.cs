using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace SaveSystem.Tests.Runtime {

    public class SaveSystemRuntimeTests {

        private const string FILE_NAME = "test";
        
        
        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator MonoTest () {
            yield return new WaitForSeconds(2);

            const string localPath = "Test Prefab";
            var testMono = Object.Instantiate(Resources.Load<TestMono>(localPath));
            var testObject = new TestObjectRuntime {
                testMono = testMono,
                prefabPath = localPath
            };
            Debug.Log("Create new object");
            yield return new WaitForSeconds(2);

            var transform = testMono.transform;
            transform.position = Random.insideUnitSphere * 5f;
            transform.rotation = Random.rotation;
            Debug.Log("Move and rotate object");
            yield return new WaitForSeconds(2);

            DataManager.SaveObject(FILE_NAME, testObject);
            testObject.testMono = null;
            Object.Destroy(testMono.gameObject);
            Debug.Log("Save object and destroy its");
            yield return new WaitForSeconds(2);

            DataManager.LoadObject(FILE_NAME, testObject);
            Debug.Log("Load object");
            yield return new WaitForSeconds(2);
            
            var method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
            Debug.Log("End test");
        }

    }

}