﻿using System.Collections;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystem.Tests {

    public class CancellationTests {

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
        public IEnumerator MeshAsyncCancelSave () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");
            await UniTask.NextFrame();

            Debug.Log("Start saving");
            var cancellationSource = new CancellationTokenSource();
            ObjectHandler objectHandler = ObjectHandlersFactory
               .Create(objects, FilePath)
               .OnThreadPool()
               .SetCancellationToken(cancellationSource.Token);

            Debug.Log("Attempt to cancel saving after starting it");
            objectHandler.SaveAsync().Forget();

            cancellationSource.Cancel();

            Assert.IsTrue(DataManager.GetDataSize() == 0);
        });


        [UnityTest]
        public IEnumerator MeshAsyncCancelLoad () => UniTask.ToCoroutine(async () => {
            var objects = new TestMesh[200];

            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Object.Instantiate(Resources.Load<TestMesh>(LpSphereName));
                objects[i].transform.position = Random.insideUnitSphere * 10;
            }

            Debug.Log("Created objects");

            Debug.Log("Start saving");
            ObjectHandler objectHandler = ObjectHandlersFactory.Create(objects, FilePath);
            HandlingResult result = await objectHandler.OnThreadPool().SaveAsync();

            if (result == HandlingResult.Success) {
                foreach (TestMesh obj in objects)
                    obj.RemoveMesh();
                Debug.Log("Meshes saved and removed");
            }

            Debug.Log("Start loading");
            var cancellationSource = new CancellationTokenSource();
            objectHandler.SetCancellationToken(cancellationSource.Token);

            Debug.Log("Attempt to cancel loading before starting it");
            cancellationSource.Cancel();

            objectHandler.LoadAsync().Forget();

            Assert.IsFalse(objects.Any(obj => obj.MeshDataIsFilling()));
        });


        [TearDown]
        public void EndTest () {
            DataManager.DeleteAllData();
            Debug.Log("End test");
        }

    }

}