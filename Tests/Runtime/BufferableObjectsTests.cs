﻿using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;

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
        public IEnumerator SimpleTest () => UniTask.ToCoroutine(async () => {
            var bufferableObjects = new BufferableObject[100];

            for (var i = 0; i < bufferableObjects.Length; i++)
                bufferableObjects[i] = CreatePrimitive();

            BufferableObjectHandler<BufferableObject> handler =
                ObjectHandlersFactory.CreateBufferableHandler(".bytes", bufferableObjects);

            await handler.SaveAsync();
            await UniTask.Delay(2000);

            foreach (BufferableObject obj in bufferableObjects)
                obj.Reset();

            await handler.LoadAsync();
            await UniTask.Delay(2000);
        });


        [TearDown]
        public void End () {
            Storage.DeleteAllData();
            Debug.Log("End test");
        }


        private BufferableObject CreatePrimitive () {
            return GameObject
               .CreatePrimitive(PrimitiveType.Cube)
               .AddComponent<BufferableObject>()
               .SetRandomTransform()
               .SetRandomColor();
        }

    }

}