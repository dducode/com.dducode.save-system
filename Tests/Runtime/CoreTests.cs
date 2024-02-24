﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using SaveSystem.CheckPoints;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

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
        public IEnumerator AutoSave () {
            var simpleObject = new BinaryObject {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            SaveSystemCore.EnabledLogs = LogLevel.All;
            SaveSystemCore.SavePeriod = 1.5f;
            SaveSystemCore.EnabledSaveEvents = SaveEvents.AutoSave;

            SaveSystemCore.RegisterSerializable(simpleObject);

            var autoSaveCompleted = false;
            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.AutoSave)
                    autoSaveCompleted = true;
            };

            yield return new WaitWhile(() => !autoSaveCompleted);
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator QuickSave () {
            var simpleObject = new BinaryObject {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            SaveSystemCore.EnabledLogs = LogLevel.All;

        #if ENABLE_LEGACY_INPUT_MANAGER
            SaveSystemCore.BindKey(KeyCode.S);
        #elif ENABLE_INPUT_SYSTEM
            SaveSystemCore.BindAction(new InputAction("save", binding: "<Keyboard>/s"));
        #else
            #error Compile error: no unity inputs enabled
        #endif

            SaveSystemCore.RegisterSerializable(simpleObject);

            var quickSaveCompleted = false;
            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.QuickSave)
                    quickSaveCompleted = true;
            };

            yield return new WaitWhile(() => !quickSaveCompleted);
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator CheckpointSave () {
            const string sphereTag = "Player";

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<TestRigidbody>();
            sphere.transform.position = Vector3.up * 10;
            sphere.tag = sphereTag;

            SaveSystemCore.EnabledLogs = LogLevel.All;
            SaveSystemCore.DestroyCheckPoints = true;
            SaveSystemCore.PlayerTag = sphereTag;

            SaveSystemCore.RegisterSerializable(new TestRigidbodyAdapter(sphere));
            CheckPointsFactory.CreateCheckPoint(Vector3.zero);

            var saveAtCheckpointCompleted = false;
            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.SaveAtCheckpoint)
                    saveAtCheckpointCompleted = true;
            };

            yield return new WaitWhile(() => !saveAtCheckpointCompleted);
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator ManySpheres () {
            const string sphereTag = "Player";
            var spheres = new List<TestRigidbodyAdapter>();

            // Spawn spheres
            for (var i = 0; i < 1000; i++) {
                var sphere = CreateSphere<TestRigidbody>();
                spheres.Add(new TestRigidbodyAdapter(sphere));

                if (i == 0)
                    sphere.tag = sphereTag;
            }

            SaveSystemCore.RegisterSerializables(spheres);
            SaveSystemCore.ConfigureParameters(
                SaveEvents.AutoSave | SaveEvents.OnFocusLost, false, LogLevel.All,
                true, sphereTag, 3
            );

            var testStopped = false;

        #if ENABLE_LEGACY_INPUT_MANAGER
            SaveSystemCore.BindKey(KeyCode.S);
        #elif ENABLE_INPUT_SYSTEM
            SaveSystemCore.BindAction(new InputAction("save", binding: "<Keyboard>/s"));
        #else
            #error Compile error: no unity inputs enabled
        #endif

            SaveSystemCore.OnSaveEnd += saveType => {
                if (saveType == SaveType.QuickSave)
                    testStopped = true;
            };

            yield return new WaitWhile(() => !testStopped);
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator ParallelSaving ([ValueSource(nameof(parallelConfig))] bool isParallel) {
            SaveSystemCore.EnabledLogs = LogLevel.All;

            for (var i = 0; i < 5; i++) {
                var meshes = new List<TestMeshAdapter>();

                for (var j = 0; j < 50; j++)
                    meshes.Add(new TestMeshAdapter(CreateSphere<TestMesh>()));

                SaveSystemCore.RegisterSerializables(meshes);
                yield return new WaitForEndOfFrame();
            }

            var saveIsCompleted = false;

            SaveSystemCore.IsParallel = isParallel;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            SaveSystemCore.SaveAsync(() => saveIsCompleted = true);
            yield return new WaitWhile(() => !saveIsCompleted);
            stopwatch.Stop();
            Debug.Log($"<color=green>Saving took milliseconds: {stopwatch.ElapsedMilliseconds}</color>");
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator Quitting () {
            var spheres = new List<TestMeshAdapter>();

            // Spawn spheres
            for (var i = 0; i < 250; i++)
                spheres.Add(new TestMeshAdapter(CreateSphere<TestMesh>()));

            SaveSystemCore.EnabledLogs = LogLevel.All;
            SaveSystemCore.EnabledSaveEvents = SaveEvents.OnExit;
            SaveSystemCore.RegisterSerializables(spheres);
            yield return new WaitForEndOfFrame();
            Application.Quit();
        }


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