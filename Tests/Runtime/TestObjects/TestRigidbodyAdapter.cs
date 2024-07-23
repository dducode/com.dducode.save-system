using SaveSystemPackage.BinaryHandlers;
using UnityEngine;

namespace SaveSystemPackage.Tests.TestObjects {

    public class TestRigidbodyAdapter : ISerializationAdapter<TestRigidbody> {

        public TestRigidbody Target { get; }


        public TestRigidbodyAdapter (TestRigidbody provider) {
            Target = provider;
        }


        public void Serialize (SaveWriter writer) {
            writer.Write(Target.position);
        }


        public void Deserialize (SaveReader reader, int previousVersion) {
            Target.position = reader.Read<Vector3>();
        }

    }

}