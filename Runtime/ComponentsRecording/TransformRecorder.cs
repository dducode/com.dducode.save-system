using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [DisallowMultipleComponent]
    [AddComponentMenu("Save System/Transform Recorder")]
    public class TransformRecorder : ComponentRecorder {

        [SerializeField]
        private Properties includedProperties = Properties.Position | Properties.EulerAngles;

        public Transform Target { get; private set; }
        private CharacterController m_characterController;
        private SerializationScope m_scope;


        public override async Task Initialize (SerializationScope scope) {
            Target = transform;
            m_scope = scope;
            m_characterController = GetComponent<CharacterController>();
            SetData(await m_scope.LoadData<TransformData>(Id, SaveSystem.exitCancellation.Token));
            m_scope.OnSave += async _ => await m_scope.SaveData(Id, GetData(), SaveSystem.exitCancellation.Token);
        }


        public override string ToString () {
            return $"{gameObject.name} Transform Recorder: {{ included properties: {includedProperties} }}";
        }


        private TransformData GetData () {
            return Target;
        }


        private void SetData (TransformData data) {
            if (data == null)
                return;

            if (m_characterController != null)
                m_characterController.enabled = false;

            if (includedProperties.HasFlag(Properties.Position))
                Target.position = data.position;
            if (includedProperties.HasFlag(Properties.LocalPosition))
                Target.localPosition = data.localPosition;

            if (includedProperties.HasFlag(Properties.EulerAngles))
                Target.eulerAngles = data.eulerAngles;
            if (includedProperties.HasFlag(Properties.LocalEulerAngles))
                Target.localEulerAngles = data.localEulerAngler;

            if (includedProperties.HasFlag(Properties.Rotation))
                Target.rotation = data.rotation;
            if (includedProperties.HasFlag(Properties.LocalRotation))
                Target.localRotation = data.localRotation;

            if (includedProperties.HasFlag(Properties.LocalScale))
                Target.localScale = data.scale;

            if (m_characterController != null)
                m_characterController.enabled = true;
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