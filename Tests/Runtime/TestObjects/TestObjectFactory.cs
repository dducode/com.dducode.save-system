using System.Diagnostics.Contracts;
using UnityEngine;

namespace SaveSystemPackage.Tests.TestObjects {

    public class TestObjectFactory : IObjectFactory<TestObject> {

        private readonly PrimitiveType m_primitiveType;


        public TestObjectFactory (PrimitiveType primitiveType) {
            m_primitiveType = primitiveType;
        }


        [Pure]
        public TestObject CreateObject () {
            var gameObject = GameObject.CreatePrimitive(m_primitiveType);
            gameObject.AddComponent<TestObject>().SetRandomTransform().SetRandomColor();
            return gameObject.AddComponent<TestObject>();
        }

    }

}