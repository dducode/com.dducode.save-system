using System;
using SaveSystemPackage.BinaryHandlers;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [DisallowMultipleComponent]
    [AddComponentMenu("Save System/Transform Recorder")]
    public class TransformRecorder : ComponentRecorder, ISerializationAdapter<Transform> {

        [SerializeField]
        private Properties includedProperties = Properties.Position | Properties.Rotation;

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
            Rotation = 4,
            LocalRotation = 8,
            LocalScale = 16,
            All = ~0

        }

    }

}