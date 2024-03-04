using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem.Internal.Extensions {

    public static class SceneExtensions {

        public static GameObject FindWithTag (this Scene scene, string tag) {
            return scene.GetRootGameObjects().FirstOrDefault(gameObject => gameObject.CompareTag(tag));
        }

    }

}