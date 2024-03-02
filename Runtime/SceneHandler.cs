using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SaveSystem {

    public abstract class SceneHandler : MonoBehaviour {

        public SceneSerializationContext sceneContext;
        public abstract UniTask StartScene ();

    }



    public abstract class SceneHandler<TData> : MonoBehaviour {

        public SceneSerializationContext sceneContext;
        public abstract UniTask StartScene (TData data);

    }

}