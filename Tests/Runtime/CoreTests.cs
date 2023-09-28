using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.CheckPoints;
using SaveSystem.Core;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using SaveType = SaveSystem.Core.SaveType;

namespace SaveSystem.Tests {

    public class CoreTests {

        private const string FilePath = "test.bytes";

        public static bool[] parallelConfig = {true, false};


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator AutoSave () => UniTask.ToCoroutine(async () => {
            var simpleObject = new BinaryObject {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.SavePeriod = 1.5f;
            SaveSystemCore.EnabledSaveEvents = SaveEvents.AutoSave;

            ObjectHandlersFactory.RegisterImmediately = true;
            ObjectHandlersFactory.CreateHandler(FilePath, simpleObject);

            var autoSaveCompleted = false;
            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.AutoSave)
                    autoSaveCompleted = true;
            };

            await UniTask.WaitWhile(() => !autoSaveCompleted);
            Assert.Greater(Storage.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator QuickSave () => UniTask.ToCoroutine(async () => {
            var simpleObject = new BinaryObject {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.BindKey(KeyCode.S);

            ObjectHandlersFactory.RegisterImmediately = true;
            ObjectHandlersFactory.CreateHandler(FilePath, simpleObject);

            var quickSaveCompleted = false;
            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.QuickSave)
                    quickSaveCompleted = true;
            };

            await UniTask.WaitWhile(() => !quickSaveCompleted);
            Assert.Greater(Storage.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator CheckpointSave () => UniTask.ToCoroutine(async () => {
            const string sphereTag = "Player";

            var sphere = CreateSphere<TestRigidbody>();
            sphere.tag = sphereTag;

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.DestroyCheckPoints = true;
            SaveSystemCore.PlayerTag = sphereTag;

            ObjectHandlersFactory.RegisterImmediately = true;
            ObjectHandlersFactory.CreateHandler(FilePath, sphere);

            CheckPointsFactory.CreateCheckPoint(Vector3.zero);

            var saveAtCheckpointCompleted = false;
            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.SaveAtCheckpoint)
                    saveAtCheckpointCompleted = true;
            };

            await UniTask.WaitWhile(() => !saveAtCheckpointCompleted);
            Assert.Greater(Storage.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator ManySpheres () => UniTask.ToCoroutine(async () => {
            const string sphereTag = "Player";
            var spheres = new List<TestRigidbody>();
            var asyncSpheres = new List<TestRigidbodyAsync>();

            // Spawn spheres
            for (var i = 0; i < 500; i++) {
                var sphere = CreateSphere<TestRigidbody>();
                spheres.Add(sphere);

                var asyncSphere = CreateSphere<TestRigidbodyAsync>();
                asyncSpheres.Add(asyncSphere);

                if (i == 0)
                    sphere.tag = sphereTag;
            }

            // Create checkpoints
            // for (var i = 0; i < 1000; i++)
            // CheckPointsFactory.CreateCheckPoint(Random.insideUnitSphere * 10);

            ObjectHandlersFactory.RegisterImmediately = true;
            ObjectHandlersFactory.CreateHandler("spheres.bytes", spheres);
            ObjectHandlersFactory.CreateAsyncHandler("async_spheres.bytes", asyncSpheres);

            SaveSystemCore.ConfigureParameters(
                SaveEvents.AutoSave | SaveEvents.OnFocusLost, false, true,
                true, sphereTag, 3
            );

            var testStopped = false;

            SaveSystemCore.BindKey(KeyCode.S);
            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.QuickSave)
                    testStopped = true;
            };

            await UniTask.WaitWhile(() => !testStopped);
            Assert.Greater(Storage.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator ParallelSaving ([ValueSource(nameof(parallelConfig))] bool isParallel) =>
            UniTask.ToCoroutine(async () => {
                ObjectHandlersFactory.RegisterImmediately = true;
                SaveSystemCore.DebugEnabled = true;

                for (var i = 0; i < 5; i++) {
                    var meshes = new List<TestMesh>();
                    var asyncMeshes = new List<TestMeshAsyncThreadPool>();

                    for (var j = 0; j < 50; j++) {
                        var testMesh = CreateSphere<TestMesh>();
                        meshes.Add(testMesh);

                        var asyncMesh = CreateSphere<TestMeshAsyncThreadPool>();
                        asyncMeshes.Add(asyncMesh);
                    }

                    ObjectHandlersFactory.CreateHandler($"test_mesh_{i}.bytes", meshes);
                    ObjectHandlersFactory.CreateAsyncHandler($"test_async_mesh_{i}.bytes", asyncMeshes);
                    await UniTask.NextFrame();
                }

                var saveIsCompleted = false;

                SaveSystemCore.IsParallel = isParallel;
                SaveSystemCore.OnSaveEnd += saveType => {
                    if (saveType == SaveType.QuickSave)
                        saveIsCompleted = true;
                };

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                SaveSystemCore.QuickSave();
                await UniTask.WaitWhile(() => !saveIsCompleted);
                stopwatch.Stop();
                Debug.Log($"<color=green>Saving took milliseconds: {stopwatch.ElapsedMilliseconds}</color>");
                Assert.Greater(Storage.GetDataSize(), 0);
            });


        [UnityTest]
        public IEnumerator Quitting () => UniTask.ToCoroutine(async () => {
            var spheres = new List<TestMesh>();
            var asyncSpheres = new List<TestMeshAsyncThreadPool>();

            // Spawn spheres
            for (var i = 0; i < 250; i++) {
                var sphere = CreateSphere<TestMesh>();
                spheres.Add(sphere);

                var asyncSphere = CreateSphere<TestMeshAsyncThreadPool>();
                asyncSpheres.Add(asyncSphere);
            }

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.EnabledSaveEvents = SaveEvents.OnExit;
            ObjectHandlersFactory.RegisterImmediately = true;
            ObjectHandlersFactory.CreateHandler("spheres.bytes", spheres);
            ObjectHandlersFactory.CreateAsyncHandler("async_spheres.bytes", asyncSpheres);
            await UniTask.NextFrame();
            Application.Quit();
        });


        [TearDown]
        public void EndTest () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }


        private T CreateSphere<T> () where T : Component {
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitive.transform.position = Random.insideUnitSphere * 10;
            return primitive.AddComponent<T>();
        }

    }

}