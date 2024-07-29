using System;
using SaveSystemPackage.Serialization;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [DisallowMultipleComponent]
    [AddComponentMenu("Save System/Transform Recorder")]
    public class TransformRecorder : ComponentRecorder, ISerializationAdapter<Transform> {

        [SerializeField]
        private Properties includedProperties = Properties.Position | Properties.EulerAngles;

        public Transform Target { get; private set; }
        private CharacterController m_characterController;


        private void Awake () {
            Target = transform;
            m_characterController = GetComponent<CharacterController>();
        }


        public override void Serialize (SaveWriter writer) {
            if (includedProperties.HasFlag(Properties.Position))
                writer.Write(Target.position);
            if (includedProperties.HasFlag(Properties.LocalPosition))
                writer.Write(Target.localPosition);

            if (includedProperties.HasFlag(Properties.EulerAngles))
                writer.Write(Target.eulerAngles);
            if (includedProperties.HasFlag(Properties.LocalEulerAngles))
                writer.Write(Target.localEulerAngles);

            if (includedProperties.HasFlag(Properties.Rotation))
                writer.Write(Target.rotation);
            if (includedProperties.HasFlag(Properties.LocalRotation))
                writer.Write(Target.localRotation);

            if (includedProperties.HasFlag(Properties.LocalScale))
                writer.Write(Target.localScale);
        }


        public override void Deserialize (SaveReader reader, int previousVersion) {
            if (m_characterController != null)
                m_characterController.enabled = false;

            if (includedProperties.HasFlag(Properties.Position))
                Target.position = reader.Read<Vector3>();
            if (includedProperties.HasFlag(Properties.LocalPosition))
                Target.localPosition = reader.Read<Vector3>();

            if (includedProperties.HasFlag(Properties.EulerAngles))
                Target.eulerAngles = reader.Read<Vector3>();
            if (includedProperties.HasFlag(Properties.LocalEulerAngles))
                Target.localEulerAngles = reader.Read<Vector3>();

            if (includedProperties.HasFlag(Properties.Rotation))
                Target.rotation = reader.Read<Quaternion>();
            if (includedProperties.HasFlag(Properties.LocalRotation))
                Target.localRotation = reader.Read<Quaternion>();

            if (includedProperties.HasFlag(Properties.LocalScale))
                Target.localScale = reader.Read<Vector3>();

            if (m_characterController != null)
                m_characterController.enabled = true;
        }


        public override string ToString () {
            return $"{gameObject.name} Transform Recorder: {{ included properties: {includedProperties} }}";
        }


        [Flags]
        private enum Properties {

            None = 0,
            Position = 1,
            LocalPosition = 2,
            EulerAngles = 4,
            LocalEulerAngles = 8,
            Rotation = 16,
            LocalRotation = 32,
            LocalScale = 64,
            All = ~0

        }

    }

}