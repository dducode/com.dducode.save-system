using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.CheckPoints;
using SaveSystem.Core;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;

namespace SaveSystem.Tests {

    public class CoreTests {

        private const string FilePath = "test.bytes";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator AutoSaveTest () => UniTask.ToCoroutine(async () => {
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
            await UniTask.Delay(2000);
            Assert.Greater(DataManager.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator QuickSaveTest () => UniTask.ToCoroutine(async () => {
            var simpleObject = new BinaryObject {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.RegisterPersistentObject(simpleObject, FilePath);

            await UniTask.WaitWhile(() => {
                if (Input.GetKey(KeyCode.S)) {
                    SaveSystemCore.QuickSave();
                    return false;
                }

                return true;
            });

            Assert.Greater(DataManager.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator CheckpointSaveTest () => UniTask.ToCoroutine(async () => {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = Vector3.up * 10;
            var sphereComponent = sphere.AddComponent<TestRigidbody>();

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.RegisterPersistentObject(sphereComponent, FilePath);
            SaveSystemCore.DestroyCheckPoints = true;
            SaveSystemCore.PlayerTag = "Player";

            CheckPointsCreator.CreateCheckPoint(Vector3.zero);
            var wasTriggeredCheckpoint = false;

            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.SaveAtCheckpoint)
                    wasTriggeredCheckpoint = true;
            };

            await UniTask.WaitWhile(() => !wasTriggeredCheckpoint);
            Assert.Greater(DataManager.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator ManySpheresTest () => UniTask.ToCoroutine(async () => {
            var spheres = new List<TestRigidbody>();

            // Spawn spheres
            for (var i = 0; i < 100; i++) {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = Random.insideUnitSphere * 10;

                if (i == 0)
                    sphere.tag = "Player";

                var component = sphere.AddComponent<TestRigidbody>();
                spheres.Add(component);
            }

            // Create checkpoints
            for (var i = 0; i < 100; i++)
                CheckPointsCreator.CreateCheckPoint(Random.insideUnitSphere * 10);

            ObjectHandler handler = HandlersProvider
               .CreateObjectHandler(spheres, FilePath)
               .OnComplete(result => {
                    if (result)
                        Debug.Log("<b>Test</b>: Successful save spheres");
                    else
                        Debug.LogError("<b>Test</b>: Failed save spheres");
                });

            SaveSystemCore.RegisterObjectHandler(handler);

            SaveSystemCore.ConfigureParameters(
                true, 3, true,
                true, true, "Player"
            );

            SaveSystemCore.OnSaveEnd += saveType => {
                switch (saveType) {
                    case SaveType.AutoSave:
                        Debug.Log("<b>Test</b>: Successful auto save");
                        break;
                    case SaveType.QuickSave:
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

            await UniTask.WaitWhile(() => {
                if (Input.GetKey(KeyCode.S)) {
                    SaveSystemCore.QuickSave();
                    return false;
                }

                return true;
            });
            
            Assert.Greater(DataManager.GetDataSize(), 0);
        });


        [UnityTest]
        public IEnumerator QuittingTest () => UniTask.ToCoroutine(async () => {
            var spheres = new List<TestRigidbody>();

            // Spawn spheres
            for (var i = 0; i < 100; i++) {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = Random.insideUnitSphere * 10;
                var component = sphere.AddComponent<TestRigidbody>();
                spheres.Add(component);
            }

            SaveSystemCore.DebugEnabled = true;
            SaveSystemCore.AsyncSaveEnabled = true;
            ObjectHandler handler = HandlersProvider.CreateObjectHandler(spheres, FilePath);
            handler.ObserveProgress(new TestProgress());
            SaveSystemCore.RegisterObjectHandler(handler);
            await UniTask.Delay(1000);
        });

    }

}