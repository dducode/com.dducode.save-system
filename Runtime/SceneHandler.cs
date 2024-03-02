using UnityEngine;

namespace SaveSystem {

    public abstract class SceneHandler : MonoBehaviour {

        public SceneSerializationContext sceneContext;
        public abstract void StartScene ();

    }



    public abstract class SceneHandler<TData> : MonoBehaviour {

        public SceneSerializationContext sceneContext;
        public abstract void StartScene (TData data);

    }

}