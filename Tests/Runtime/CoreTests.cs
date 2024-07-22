﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.CheckPoints;
using SaveSystemPackage.Internal.Cryptography;
using SaveSystemPackage.Security;
using SaveSystemPackage.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace SaveSystemPackage.Tests {

    public class CoreTests {

        private const string Password = "password";
        private const string SaltKey = "salt";
        private const string AuthHashKey = "1593f666-b209-4e70-af46-58dd9ca791c9";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);

            SaveSystem.EnabledLogs = LogLevel.All;
            SaveSystem.SelectedSaveProfile = new TestSaveProfile();
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator AutoSave () {
            TestObject simpleObject = new TestObjectFactory(PrimitiveType.Cube).CreateObject();

            SaveSystem.SavePeriod = 1.5f;
            SaveSystem.EnabledSaveEvents = SaveEvents.AutoSave;

            SaveSystem.RegisterSerializable(nameof(simpleObject), new TestObjectAdapter(simpleObject));

            var autoSaveCompleted = false;
            SaveSystem.OnSaveEnd += saveType => {
                if (saveType == SaveType.AutoSave)
                    autoSaveCompleted = true;
            };

            yield return new WaitWhile(() => !autoSaveCompleted);
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator QuickSave () {
            TestObject simpleObject = new TestObjectFactory(PrimitiveType.Cube).CreateObject();

        #if ENABLE_LEGACY_INPUT_MANAGER
            SaveSystem.BindKey(KeyCode.S);
        #elif ENABLE_INPUT_SYSTEM
            SaveSystemCore.BindAction(new InputAction("save", binding: "<Keyboard>/s"));
        #else
            #error Compile error: no unity inputs enabled
        #endif

            SaveSystem.RegisterSerializable(nameof(simpleObject), new TestObjectAdapter(simpleObject));

            var quickSaveCompleted = false;
            SaveSystem.OnSaveEnd += saveType => {
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

            SaveSystem.PlayerTag = sphereTag;

            SaveSystem.RegisterSerializable(nameof(sphere), new TestRigidbodyAdapter(sphere));
            CheckPointsFactory.CreateCheckPoint(Vector3.zero);

            var saveAtCheckpointCompleted = false;
            SaveSystem.OnSaveEnd += saveType => {
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
            var factory = new TestObjectDecorator<TestRigidbody>(new TestObjectFactory(PrimitiveType.Sphere));

            // Spawn spheres
            for (var i = 0; i < 1000; i++) {
                TestRigidbody sphere = factory.CreateObject();
                spheres.Add(new TestRigidbodyAdapter(sphere));

                if (i == 0)
                    sphere.tag = sphereTag;
            }

            SaveSystem.RegisterSerializables(nameof(spheres), spheres);
            var settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
            settings.encryption = false;
            settings.authentication = false;
            settings.enabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnFocusLost;
            settings.savePeriod = 3;
            settings.enabledLogs = LogLevel.All;
            settings.playerTag = sphereTag;
            SaveSystem.ConfigureSettings(settings);

            var testStopped = false;

        #if ENABLE_LEGACY_INPUT_MANAGER
            SaveSystem.BindKey(KeyCode.S);
        #elif ENABLE_INPUT_SYSTEM
            SaveSystemCore.BindAction(new InputAction("save", binding: "<Keyboard>/s"));
        #else
            #error Compile error: no unity inputs enabled
        #endif

            SaveSystem.OnSaveEnd += saveType => {
                if (saveType == SaveType.QuickSave)
                    testStopped = true;
            };

            yield return new WaitWhile(() => !testStopped);
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator Quitting () {
            var sphereFactory = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Cube), new TestObjectProvider()
            );
            sphereFactory.CreateObjects(250);

            SaveSystem.EnabledSaveEvents = SaveEvents.OnExit;
            SaveSystem.RegisterSerializable(nameof(sphereFactory), sphereFactory);
            yield return new WaitForEndOfFrame();
            Application.Quit();
        }


        [Test, Category("Encryption"), Order(0)]
        public async Task SerializeWithEncryption () {
            var sphereFactory = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Sphere), new TestObjectProvider()
            );
            sphereFactory.CreateObjects(250);

            var generationParams = KeyGenerationParams.Default;
            generationParams.hashAlgorithm = HashAlgorithmName.SHA1;
            SaveSystem.Encrypt = true;
            SaveSystem.Cryptographer = new Cryptographer(
                new DefaultKeyProvider(Password),
                new DefaultKeyProvider(SaltKey),
                generationParams
            );

            SaveSystem.RegisterSerializable(nameof(sphereFactory), sphereFactory);
            await SaveSystem.Save();
            await UniTask.WaitForSeconds(1);
        }


        [Test, Category("Encryption"), Order(1)]
        public async Task DeserializeWithDecryption () {
            var sphereFactory = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Sphere), new TestObjectProvider()
            );

            var generationParams = KeyGenerationParams.Default;
            generationParams.hashAlgorithm = HashAlgorithmName.SHA1;
            SaveSystem.Encrypt = true;
            SaveSystem.Cryptographer = new Cryptographer(
                new DefaultKeyProvider(Password),
                new DefaultKeyProvider(SaltKey),
                generationParams
            );

            SaveSystem.RegisterSerializable(nameof(sphereFactory), sphereFactory);
            await SaveSystem.Load();
            await UniTask.WaitForSeconds(1);
        }


        [Test]
        public async Task SerializeWithAuthentication () {
            var sphereFactory = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Sphere), new TestObjectProvider()
            );
            sphereFactory.CreateObjects(250);

            SaveSystem.Authenticate = true;
            SaveSystem.AuthManager = new AuthenticationManager(HashAlgorithmName.SHA1);

            SaveSystem.RegisterSerializable(nameof(sphereFactory), sphereFactory);
            await SaveSystem.Save();
            await UniTask.WaitForSeconds(1);
        }


        [Test]
        public async Task DeserializeWithAuthentication () {
            var sphereFactory = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Sphere), new TestObjectProvider()
            );

            SaveSystem.Authenticate = true;
            SaveSystem.AuthManager = new AuthenticationManager(HashAlgorithmName.SHA1);

            SaveSystem.RegisterSerializable(nameof(sphereFactory), sphereFactory);
            await SaveSystem.Load();
            await UniTask.WaitForSeconds(1);

            PlayerPrefs.DeleteKey(AuthHashKey);
        }



        public class DataBufferTests {

            [Test, Order(0)]
            public async Task WriteToDataBuffer () {
                var factory = new TestObjectFactory(PrimitiveType.Sphere);
                TestObject testObject = factory.CreateObject();
                SaveSystem.WriteData("position", testObject.transform.position);
                Debug.Log(testObject.transform.position);
                await SaveSystem.Save();
            }


            [Test, Order(1)]
            public async Task ReadFromDataBuffer () {
                await SaveSystem.Load();
                Debug.Log(SaveSystem.ReadData<Vector3>("position"));
            }

        }



        [TearDown]
        public void EndTest () {
            Debug.Log("End test");
        }

    }

}