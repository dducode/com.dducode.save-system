using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem.Internal.Extensions {

    internal static class SceneExtensions {

        internal static GameObject FindWithTag (this Scene scene, string tag) {
            return scene.GetRootGameObjects().FirstOrDefault(gameObject => gameObject.CompareTag(tag));
        }

    }

}