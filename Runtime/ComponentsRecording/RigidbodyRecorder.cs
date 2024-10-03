using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Save System/Rigidbody Recorder")]
    public class RigidbodyRecorder : ComponentRecorder {

        [SerializeField]
        private Properties includedProperties = Properties.All;

        private SerializationScope m_scope;

        public Rigidbody Target { get; private set; }


        public override async Task Initialize (SerializationScope scope) {
            Target = GetComponent<Rigidbody>();
            m_scope = scope;
            SetData(await m_scope.LoadData<RigidbodyData>(Id, SaveSystem.exitCancellation.Token));
            m_scope.OnSave += async _ => await m_scope.SaveData(Id, GetData(), SaveSystem.exitCancellation.Token);
        }


        public override string ToString () {
            return $"{gameObject.name} Rigidbody Recorder: {{ included properties: {includedProperties} }}";
        }


        private RigidbodyData GetData () {
            return Target;
        }


        private void SetData (RigidbodyData data) {
            if (data == null)
                return;

            if (includedProperties.HasFlag(Properties.Position))
                Target.position = data.position;
            if (includedProperties.HasFlag(Properties.Rotation))
                Target.rotation = data.rotation;

            if (includedProperties.HasFlag(Properties.Velocity))
                Target.velocity = data.velocity;
            if (includedProperties.HasFlag(Properties.AngularVelocity))
                Target.angularVelocity = data.angularVelocity;

            if (includedProperties.HasFlag(Properties.IsKinematic))
                Target.isKinematic = data.isKinematic;
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