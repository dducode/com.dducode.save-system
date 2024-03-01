using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public class BufferableObjectFactory : IObjectFactory<BufferableObject> {

        public BufferableObject CreateObject () {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.AddComponent<BufferableObject>().SetRandomTransform().SetRandomColor();
            return gameObject.AddComponent<BufferableObject>();
        }

    }

}