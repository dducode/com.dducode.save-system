using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.InternalServices;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystem.Tests {

    internal sealed class MeshTests {

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
        public IEnumerator MeshSaveLoad () {
            TestMesh testMono = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
            Debug.Log("Create object");
            yield return new WaitForSeconds(2);

            ObjectHandler<TestMesh> objectHandler = ObjectHandlersFactory.Create(FilePath, testMono);
            objectHandler.Save();
            testMono.RemoveMesh();
            Debug.Log("Save and remove mesh");
            yield return new WaitForSeconds(2);

            objectHandler.Load();
            testMono.SetMesh();
            Debug.Log("Load mesh");
            yield return new WaitForSeconds(2);
        }


        [UnityTest]
        public IEnumerator MeshesSaveLoadAsyncOnPlayerLoop () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            ObjectHandler<TestMesh> objectHandler = ObjectHandlersFactory.Create(FilePath, objects);

            HandlingResult result = await objectHandler.SaveAsync();

            if (result == HandlingResult.Success) {
                foreach (TestMesh obj in objects)
                    obj.RemoveMesh();
                Debug.Log("Meshes saved and removed");
            }

            Debug.Log("Start loading");
            result = await objectHandler.LoadAsync();

            if (result == HandlingResult.Success) {
                foreach (TestMesh testMesh in objects)
                    testMesh.SetMesh();
                Debug.Log("Meshes loaded");
            }
        });


        [UnityTest]
        public IEnumerator MeshesSaveLoadAsyncOnThreadPool () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            ObjectHandler<TestMesh> objectHandler = ObjectHandlersFactory.Create(FilePath, objects);
            Debug.Log("Start saving");

            await UniTask.RunOnThreadPool(() => { objectHandler.Save(); });

            foreach (TestMesh obj in objects)
                obj.RemoveMesh();
            Debug.Log("Meshes saved and removed");

            Debug.Log("Start loading");

            await UniTask.RunOnThreadPool(() => { objectHandler.Load(); });

            foreach (TestMesh testMesh in objects)
                testMesh.SetMesh();
            Debug.Log("Meshes loaded");
        });


        [TearDown]
        public void EndTest () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }

    }

}