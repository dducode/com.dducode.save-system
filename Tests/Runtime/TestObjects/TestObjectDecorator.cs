using UnityEngine;

namespace SaveSystemPackage.Tests.TestObjects {

    public class TestObjectDecorator<TComponent> where TComponent : Component {

        private readonly TestObjectFactory m_factory;


        public TestObjectDecorator (TestObjectFactory factory) {
            m_factory = factory;
        }


        public TComponent CreateObject () {
            return m_factory.CreateObject().gameObject.AddComponent<TComponent>();
        }

    }

}