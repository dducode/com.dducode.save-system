using UnityEngine;

namespace SaveSystem {

    public abstract class SceneHandler : MonoBehaviour {

        public SceneSerializationContext sceneContext;
        public abstract void OnPreLoad ();
        public abstract void OnPostLoad ();

    }

}