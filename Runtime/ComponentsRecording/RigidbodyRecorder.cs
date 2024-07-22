using System;
using SaveSystemPackage.BinaryHandlers;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Save System/Rigidbody Recorder")]
    public class RigidbodyRecorder : MonoBehaviour, ISerializationAdapter<Rigidbody> {

        [SerializeField]
        private Properties includedProperties = Properties.All;

        public Rigidbody Target { get; private set; }


        private void Awake () {
            Target = GetComponent<Rigidbody>();
        }


        public void Serialize (SaveWriter writer) {
            if (includedProperties.HasFlag(Properties.Position))
                writer.Write(Target.position);
            if (includedProperties.HasFlag(Properties.Rotation))
                writer.Write(Target.rotation);

            if (includedProperties.HasFlag(Properties.Velocity))
                writer.Write(Target.velocity);
            if (includedProperties.HasFlag(Properties.AngularVelocity))
                writer.Write(Target.angularVelocity);

            if (includedProperties.HasFlag(Properties.IsKinematic))
                writer.Write(Target.isKinematic);
        }


        public void Deserialize (SaveReader reader, int previousVersion) {
            if (includedProperties.HasFlag(Properties.Position))
                Target.position = reader.Read<Vector3>();
            if (includedProperties.HasFlag(Properties.Rotation))
                Target.rotation = reader.Read<Quaternion>();

            if (includedProperties.HasFlag(Properties.Velocity))
                Target.velocity = reader.Read<Vector3>();
            if (includedProperties.HasFlag(Properties.AngularVelocity))
                Target.angularVelocity = reader.Read<Vector3>();

            if (includedProperties.HasFlag(Properties.IsKinematic))
                Target.isKinematic = reader.Read<bool>();
        }



        [Flags]
        private enum Properties {

            None = 0,
            Position = 1,
            Rotation = 2,
            Velocity = 4,
            AngularVelocity = 8,
            IsKinematic = 16,
            All = ~0

        }

    }

}