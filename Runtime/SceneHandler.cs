using UnityEngine;

namespace SaveSystemPackage {

    public abstract class SceneHandler : MonoBehaviour {

        public SceneSerializationScope sceneScope;
        public abstract void StartScene ();

    }



    public abstract class SceneHandler<TData> : MonoBehaviour {

        public SceneSerializationScope sceneScope;
        public abstract void StartScene (TData data);

    }

}