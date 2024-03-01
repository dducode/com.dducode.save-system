using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public class SphereFactory<T> : IObjectFactory<T> where T : Component {

        public T CreateObject () {
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitive.transform.position = Random.insideUnitSphere * 10;
            return primitive.AddComponent<T>();
        }

    }

}