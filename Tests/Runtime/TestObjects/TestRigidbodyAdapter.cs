using SaveSystem.BinaryHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public class TestRigidbodyAdapter : ISerializationAdapter<TestRigidbody> {

        public TestRigidbody Target { get; }


        public TestRigidbodyAdapter (TestRigidbody provider) {
            Target = provider;
        }


        public void Serialize (SaveWriter writer) {
            writer.Write(Target.position);
        }


        public void Deserialize (SaveReader reader) {
            Target.position = reader.Read<Vector3>();
        }

    }

}