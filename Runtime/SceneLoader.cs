using UnityEngine;

namespace SaveSystem {

    public abstract class SceneLoader : MonoBehaviour {

        public SceneSerializationContext sceneContext;
        public abstract void Setup ();

    }

}