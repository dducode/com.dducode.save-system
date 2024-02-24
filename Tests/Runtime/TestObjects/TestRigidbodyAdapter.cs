using SaveSystem.BinaryHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public class TestRigidbodyAdapter : ISerializationAdapter<TestRigidbody> {

        public TestRigidbody Target { get; }
        public bool DontDestroyOnSceneUnload => false;


        public TestRigidbodyAdapter (TestRigidbody provider) {
            Target = provider;
        }


        public void Serialize (BinaryWriter writer) {
            writer.Write(Target.position);
        }


        public void Deserialize (BinaryReader reader) {
            Target.position = reader.Read<Vector3>();
        }

    }

}