using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.CheckPoints;
using SaveSystem.Cryptography;
using SaveSystem.Internal.CryptoProviders;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace SaveSystem.Tests {

    public class CoreTests {

        public static bool[] parallelConfig = {true, false};

        private static readonly HashAlgorithm[] HashAlgorithm = {
            Cryptography.HashAlgorithm.SHA1,
            Cryptography.HashAlgorithm.SHA256,
            Cryptography.HashAlgorithm.SHA384,
            Cryptography.HashAlgorithm.SHA512
        };

        private const string Password = "password";
        private const string SaltKey = "salt";


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);

            SaveSystemCore.EnabledLogs = LogLevel.All;
            SaveSystemCore.SelectedSaveProfile = new SaveProfile {
                Name = "test", ProfileDataFolder = "test"
            };
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator AutoSave () {
            TestObject simpleObject = new TestObjectFactory(PrimitiveType.Cube).CreateObject();

            SaveSystemCore.SavePeriod = 1.5f;
            SaveSystemCore.EnabledSaveEvents = SaveEvents.AutoSave;

            SaveSystemCore.RegisterSerializable(new TestObjectAdapter(simpleObject));

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
            TestObject simpleObject = new TestObjectFactory(PrimitiveType.Cube).CreateObject();

        #if ENABLE_LEGACY_INPUT_MANAGER
            SaveSystemCore.BindKey(KeyCode.S);
        #elif ENABLE_INPUT_SYSTEM
            SaveSystemCore.BindAction(new InputAction("save", binding: "<Keyboard>/s"));
        #else
            #error Compile error: no unity inputs enabled
        #endif

            SaveSystemCore.RegisterSerializable(new TestObjectAdapter(simpleObject));

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
            var factory = new TestObjectDecorator<TestRigidbody>(new TestObjectFactory(PrimitiveType.Sphere));

            // Spawn spheres
            for (var i = 0; i < 1000; i++) {
                TestRigidbody sphere = factory.CreateObject();
                spheres.Add(new TestRigidbodyAdapter(sphere));

                if (i == 0)
                    sphere.tag = sphereTag;
            }

            SaveSystemCore.RegisterSerializables(spheres);
            var settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
            settings.enabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnFocusLost;
            settings.savePeriod = 3;
            settings.enabledLogs = LogLevel.All;
            settings.playerTag = sphereTag;
            SaveSystemCore.ConfigureSettings(settings);

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
            var factory = new TestObjectFactory(PrimitiveType.Cube);

            for (var i = 0; i < 5; i++) {
                var meshes = new List<TestObjectAdapter>();

                for (var j = 0; j < 50; j++)
                    meshes.Add(new TestObjectAdapter(factory.CreateObject()));

                SaveSystemCore.RegisterSerializables(meshes);
                yield return new WaitForEndOfFrame();
            }

            var saveIsCompleted = false;

            SaveSystemCore.IsParallel = isParallel;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            SaveSystemCore.SaveAsync(_ => saveIsCompleted = true);
            yield return new WaitWhile(() => !saveIsCompleted);
            stopwatch.Stop();
            Debug.Log($"<color=green>Saving took milliseconds: {stopwatch.ElapsedMilliseconds}</color>");
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator Quitting () {
            var sphereFactory = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Cube), new TestObjectProvider()
            );
            sphereFactory.CreateObjects(250);

            SaveSystemCore.EnabledSaveEvents = SaveEvents.OnExit;
            SaveSystemCore.RegisterSerializable(sphereFactory);
            yield return new WaitForEndOfFrame();
            Application.Quit();
        }


        [Test]
        public async Task SerializeWithEncryption ([ValueSource(nameof(HashAlgorithm))] HashAlgorithm hashAlgorithm) {
            var sphereFactory = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Sphere), new TestObjectProvider()
            );
            sphereFactory.CreateObjects(250);

            var generationParams = KeyGenerationParams.Default;
            generationParams.hashAlgorithm = hashAlgorithm;
            SaveSystemCore.Encrypt = true;
            SaveSystemCore.Cryptographer = new Cryptographer(
                new DefaultPasswordProvider(Password),
                new DefaultSaltProvider(SaltKey),
                generationParams
            );
            SaveSystemCore.RegisterSerializable(sphereFactory);
            await SaveSystemCore.SaveAsync();
            await UniTask.WaitForSeconds(1);
        }


        [Test]
        public async Task DeserializeWithDecryption (
            [ValueSource(nameof(HashAlgorithm))] HashAlgorithm hashAlgorithm
        ) {
            var sphereFactory = new DynamicObjectGroup<TestObject>(
                new TestObjectFactory(PrimitiveType.Sphere), new TestObjectProvider()
            );

            var generationParams = KeyGenerationParams.Default;
            generationParams.hashAlgorithm = hashAlgorithm;
            SaveSystemCore.Encrypt = true;
            SaveSystemCore.Cryptographer = new Cryptographer(
                new DefaultPasswordProvider(Password),
                new DefaultSaltProvider(SaltKey),
                generationParams
            );
            SaveSystemCore.RegisterSerializable(sphereFactory);
            await SaveSystemCore.LoadAsync();
            await UniTask.WaitForSeconds(1);
        }


        // TODO: write test
        public async Task WriteToDataBuffer () {
            var buffer = SaveSystemCore.DataBuffer;
        }


        [TearDown]
        public void EndTest () {
            Debug.Log("End test");
        }

    }

}