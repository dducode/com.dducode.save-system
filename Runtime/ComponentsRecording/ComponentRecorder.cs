using SaveSystemPackage.Attributes;
using SaveSystemPackage.Serialization;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    public abstract class ComponentRecorder : MonoBehaviour, IRuntimeSerializable {

        [SerializeField, NonEditable]
        private string id;

        public string Id => id;

        public abstract void Serialize (SaveWriter writer);
        public abstract void Deserialize (SaveReader reader, int previousVersion);

    }

}