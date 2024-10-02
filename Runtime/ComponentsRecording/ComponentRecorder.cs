using System.Threading.Tasks;
using SaveSystemPackage.Attributes;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    public abstract class ComponentRecorder : MonoBehaviour {

        [SerializeField, NonEditable]
        private string id;

        public string Id => id;

        public abstract Task Initialize (SerializationScope scope);

    }

}