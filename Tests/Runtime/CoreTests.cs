using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.CheckPoints;
using SaveSystemPackage.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace SaveSystemPackage.Tests {

    public class CoreTests {

        private TestProfile m_profile;


        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);

            SaveSystem.Settings.EnabledLogs = LogLevel.All;
            m_profile = SaveSystem.CreateProfile<TestProfile>("test-profile");
            SaveSystem.Game.SaveProfile = m_profile;
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator PeriodicSave () {
            TestObject simpleObject = new TestObjectFactory(PrimitiveType.Cube).CreateObject();

            SaveSystem.Settings.SavePeriod = 1.5f;
            SaveSystem.Settings.EnabledSaveEvents = SaveEvents.PeriodicSave;

            var testObjectAdapter = new TestObjectAdapter(simpleObject);
            m_profile.OnSave += async _ => await m_profile.SaveData(testObjectAdapter.GetData());

            var autoSaveCompleted = false;
            SaveSystem.OnSaveEnd += saveType => {
                if (saveType == SaveType.PeriodicSave)
                    autoSaveCompleted = true;
            };

            yield return new WaitWhile(() => !autoSaveCompleted);
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        [UnityTest]
        public IEnumerator QuickSave () {
            TestObject simpleObject = new TestObjectFactory(PrimitiveType.Cube).CreateObject();

        #if ENABLE_LEGACY_INPUT_MANAGER
            SaveSystem.Settings.QuickSaveKey = KeyCode.S;
        #elif ENABLE_INPUT_SYSTEM
            SaveSystemCore.BindAction(new InputAction("save", binding: "<Keyboard>/s"));
        #else
            #error Compile error: no unity inputs enabled
        #endif

            var testObjectAdapter = new TestObjectAdapter(simpleObject);
            m_profile.OnSave += async _ => await m_profile.SaveData(testObjectAdapter.GetData());

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

            SaveSystem.Settings.PlayerTag = sphereTag;

            var testRigidbodyAdapter = new TestRigidbodyAdapter(sphere);
            m_profile.OnSave += async _ => await m_profile.SaveData(testRigidbodyAdapter.GetData());
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
            var factory = new TestObjectDecorator<TestRigidbody>(new TestObjectFactory(PrimitiveType.Sphere));

            // Spawn spheres
            for (var i = 0; i < 1000; i++) {
                TestRigidbody sphere = factory.CreateObject();
                var testRigidbodyAdapter = new TestRigidbodyAdapter(sphere);
                m_profile.OnSave += async _ => await m_profile.SaveData(testRigidbodyAdapter.GetData());

                if (i == 0)
                    sphere.tag = sphereTag;
            }

            m_profile.OverriddenSettings.Encrypt = false;
            m_profile.OverriddenSettings.CompressFiles = false;
            SaveSystem.Settings.EnabledSaveEvents = SaveEvents.PeriodicSave | SaveEvents.OnFocusLost;
            SaveSystem.Settings.SavePeriod = 3;
            SaveSystem.Settings.EnabledLogs = LogLevel.All;
            SaveSystem.Settings.PlayerTag = sphereTag;

            var testStopped = false;

        #if ENABLE_LEGACY_INPUT_MANAGER
            SaveSystem.Settings.QuickSaveKey = KeyCode.S;
        #elif ENABLE_INPUT_SYSTEM
            SaveSystemCore.QuickSaveAction = new InputAction("save", binding: "<Keyboard>/s");
        #else
            #warning: no unity inputs enabled
        #endif

            SaveSystem.OnSaveEnd += saveType => {
                if (saveType == SaveType.QuickSave)
                    testStopped = true;
            };

            yield return new WaitWhile(() => !testStopped);
            Assert.Greater(Storage.GetDataSize(), 0);
        }


        public class DataBufferTests {

            [Test, Order(0)]
            public async Task WriteToDataBuffer () {
                var factory = new TestObjectFactory(PrimitiveType.Sphere);
                TestObject testObject = factory.CreateObject();
                SaveSystem.Game.Data.Write("position", testObject.transform.position);
                Debug.Log(testObject.transform.position);
                // await SaveSystem.Game.Save();
            }


            [Test, Order(1)]
            public async Task ReadFromDataBuffer () {
                // await SaveSystem.Game.Load();
                Debug.Log(SaveSystem.Game.Data.Read<Vector3>("position"));
            }

        }


        [TearDown]
        public void EndTest () {
            SaveSystem.DeleteProfile(m_profile);
            Debug.Log("End test");
        }

    }

}