using System.Collections;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAwaiter = Cysharp.Threading.Tasks.UniTask<SaveSystem.Handlers.HandlingResult>.Awaiter;
#else
using TaskAwaiter = System.Runtime.CompilerServices.TaskAwaiter<SaveSystem.Handlers.HandlingResult>;
#endif

namespace SaveSystem.Tests {

    public class BufferableObjectsTests {

        [SetUp]
        public void Start () {
            var camera = new GameObject();
            camera.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            Debug.Log("Start test");
        }


        [UnityTest]
        public IEnumerator SimpleTest () {
            var bufferableObjects = new Storable[100];

            for (var i = 0; i < bufferableObjects.Length; i++)
                bufferableObjects[i] = CreatePrimitive();

            SmartHandler<Storable> handler =
                ObjectHandlersFactory.CreateSmartHandler(".bytes", bufferableObjects);

            TaskAwaiter saveAsync = handler.SaveAsync().GetAwaiter();
            var awaiter = new WaitWhile(() => !saveAsync.IsCompleted);
            yield return awaiter;
            yield return new WaitForSeconds(2);

            foreach (Storable obj in bufferableObjects)
                obj.Reset();

            TaskAwaiter loadAsync = handler.LoadAsync().GetAwaiter();
            awaiter = new WaitWhile(() => !loadAsync.IsCompleted);
            yield return awaiter;
            yield return new WaitForSeconds(2);
        }


        [TearDown]
        public void End () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }


        private Storable CreatePrimitive () {
            return GameObject
               .CreatePrimitive(PrimitiveType.Cube)
               .AddComponent<Storable>()
               .SetRandomTransform()
               .SetRandomColor();
        }

    }

}