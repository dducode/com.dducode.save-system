using System.Threading.Tasks;
using SaveSystemPackage.ComponentsRecording;
using SaveSystemPackage.Profiles;
using SaveSystemPackage.Providers;
using SaveSystemPackage.Storages;
using UnityEngine;
using Directory = SaveSystemPackage.Internal.Directory;

namespace SaveSystemPackage {

    [AddComponentMenu("Save System/Scene Serialization Scope")]
    [DisallowMultipleComponent]
    public class SceneSerializationScopeComponent : MonoBehaviour {

        [SerializeField]
        private string id;

        public SceneSerializationScope SceneScope { get; private set; }


        private async void Awake () {
            SceneScope = new SceneSerializationScope {
                Name = $"{gameObject.scene.name} scene scope"
            };

            SaveProfile profile = SaveSystem.Game.SaveProfile;

            if (profile == null) {
                Directory directory = Storage.ScenesDirectory.GetOrCreateDirectory(id);
                SceneScope.KeyProvider = new CompositeKeyStore(SaveSystem.Game.KeyProvider, directory.Name);
                SceneScope.DataStorage = new FileSystemStorage(directory, "scenedata");
                SaveSystem.Game.SceneScope = SceneScope;
            }
            else {
                Directory directory = profile.directory.GetOrCreateDirectory(id);
                SceneScope.KeyProvider = new CompositeKeyStore(profile.KeyProvider, directory.Name);
                SceneScope.DataStorage = new FileSystemStorage(directory, "scenedata");
                profile.SceneScope = SceneScope;
            }

            await RegisterRecorders();
        }


        private void OnValidate () {
            if (string.IsNullOrEmpty(id))
                id = gameObject.GetInstanceID().ToString();
        }


        private async Task RegisterRecorders () {
            foreach (ComponentRecorder recorder in FindObjectsByType<ComponentRecorder>(FindObjectsSortMode.None))
                await recorder.Initialize(SceneScope);
        }

    }

}