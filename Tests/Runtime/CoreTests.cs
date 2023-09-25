using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.CheckPoints;
using SaveSystem.Core;
using SaveSystem.Handlers;
using SaveSystem.InternalServices;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace SaveSystem.Tests {

    public class CoreTests {

        private const string FilePath = "test.bytes";

        public static SaveMode[] saveModes = {SaveMode.Simple, SaveMode.Async, SaveMode.Parallel};


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
            SaveSystemCore.RegisterPersistentObject(simpleObject, FilePath);
            SaveSystemCore.SavePeriod = 1.5f;
            SaveSystemCore.AutoSaveEnabled = true;

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
            SaveSystemCore.RegisterPersistentObject(simpleObject, FilePath);
            SaveSystemCore.BindKey(KeyCode.S);

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

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = Vector3.up * 10;
            sphere.tag = sphereTag;
            var sphereComponent = sphere.AddComponent<TestRigidbody>();

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.RegisterPersistentObject(sphereComponent, FilePath);
            SaveSystemCore.DestroyCheckPoints = true;
            SaveSystemCore.PlayerTag = sphereTag;
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
            var spheres = new List<TestRigidbody>();

            // Spawn spheres
            for (var i = 0; i < 1000; i++) {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = Random.insideUnitSphere * 10;

                if (i == 0)
                    sphere.tag = "Player";

                var component = sphere.AddComponent<TestRigidbody>();
                spheres.Add(component);
            }

            // Create checkpoints
            for (var i = 0; i < 1000; i++)
                CheckPointsFactory.CreateCheckPoint(Random.insideUnitSphere * 10);

            ObjectHandlersFactory.RegisterImmediately = true;
            ObjectHandlersFactory.CreateHandler(FilePath, spheres);

            SaveSystemCore.ConfigureParameters(
                true, true, true,
                SaveMode.Async, "Player", 3, AsyncMode.OnThreadPool
            );

            var testStopped = false;

            SaveSystemCore.OnSaveEnd += saveType => {
                switch (saveType) {
                    case SaveType.AutoSave:
                        Debug.Log("<b>Test</b>: Successful auto save");
                        break;
                    case SaveType.QuickSave:
                        testStopped = true;
                        Debug.Log("<b>Test</b>: Successful quick-save");
                        break;
                    case SaveType.SaveAtCheckpoint:
                        Debug.Log("<b>Test</b>: Successful save at checkpoint");
                        break;
                    case SaveType.OnExit:
                        Debug.Log("<b>Test</b>: Successful save on exit");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(saveType), saveType, null);
                }
            };

            SaveSystemCore.BindKey(KeyCode.S);

            await UniTask.WaitWhile(() => !testStopped);
            Assert.Greater(Storage.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator DifferentSavingModes ([ValueSource(nameof(saveModes))] SaveMode saveMode) =>
            UniTask.ToCoroutine(async () => {
                ObjectHandlersFactory.RegisterImmediately = true;
                SaveSystemCore.DebugEnabled = true;

                for (var i = 0; i < 10; i++) {
                    var spheres = new List<TestMesh>();

                    for (var j = 0; j < 100; j++) {
                        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.position = Random.insideUnitSphere * 10;
                        var sphereComponent = sphere.AddComponent<TestMesh>();
                        spheres.Add(sphereComponent);
                    }

                    ObjectHandlersFactory.CreateHandler($"test_{i}.bytes", spheres);
                    await UniTask.NextFrame();
                }

                SaveSystemCore.SaveMode = saveMode;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                await SaveSystemCore.SaveBeforeExit(false);
                stopwatch.Stop();
                Debug.Log($"<color=green>Saving took milliseconds: {stopwatch.ElapsedMilliseconds}</color>");
                Assert.Greater(Storage.GetDataSize(), 0);
            });


        [UnityTest]
        public IEnumerator Quitting () => UniTask.ToCoroutine(async () => {
            var spheres = new List<TestRigidbody>();

            // Spawn spheres
            for (var i = 0; i < 1000; i++) {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = Random.insideUnitSphere * 10;
                var component = sphere.AddComponent<TestRigidbody>();
                spheres.Add(component);
            }

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.SaveMode = SaveMode.Simple;
            ObjectHandlersFactory.RegisterImmediately = true;
            ObjectHandlersFactory.CreateHandler(FilePath, spheres);
            await SaveSystemCore.SaveBeforeExit(true);
            Assert.Greater(Storage.GetDataSize(), 0);
        });


        [TearDown]
        public void EndTest () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }

    }

}