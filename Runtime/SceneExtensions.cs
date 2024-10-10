using System.Linq;
using SaveSystemPackage.Internal;
using UnityEngine.SceneManagement;

namespace SaveSystemPackage {

    public static class SceneExtensions {

        public static SceneSerializationContext GetSerializationScope (this Scene scene) {
            return scene
               .GetRootGameObjects()
               .FirstOrDefault(g => g.CompareTag(Tags.SceneScopeTag))
              ?.GetComponent<SceneSerializationScopeComponent>()
               .SceneContext;
        }

    }

}