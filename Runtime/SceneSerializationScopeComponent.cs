using SaveSystemPackage.Profiles;
using SaveSystemPackage.Providers;
using SaveSystemPackage.Storages;
using UnityEngine;
using UnityEngine.Events;
using Directory = SaveSystemPackage.Internal.Directory;

namespace SaveSystemPackage {

    [AddComponentMenu("Save System/Scene Serialization Scope")]
    [DisallowMultipleComponent]
    public class SceneSerializationScopeComponent : MonoBehaviour {

        [SerializeField]
        private string id;

        [SerializeField]
        private UnityEvent onInitialized;

        public SceneSerializationScope SceneScope { get; private set; }


        private void Awake () {
            SceneScope = new SceneSerializationScope {
                Name = $"{gameObject.scene.name} scene scope",
                Serializer = SaveSystem.Settings.SharedSerializer
            };

            SaveProfile profile = SaveSystem.Game.SaveProfile;
            string fileExtension = SaveSystem.Settings.SharedSerializer.GetFormatCode();

            if (profile == null) {
                Directory directory = Storage.ScenesDirectory.GetOrCreateDirectory(id);
                SceneScope.KeyProvider = new CompositeKeyStore(SaveSystem.Game.KeyProvider, directory.Name);
                SceneScope.DataStorage = new FileSystemStorage(directory, fileExtension);
                SaveSystem.Game.SceneScope = SceneScope;
            }
            else {
                Directory directory = profile.directory.GetOrCreateDirectory(id);
                SceneScope.KeyProvider = new CompositeKeyStore(profile.KeyProvider, directory.Name);
                SceneScope.DataStorage = new FileSystemStorage(directory, fileExtension);
                profile.SceneScope = SceneScope;
            }

            onInitialized?.Invoke();
        }


        private void OnValidate () {
            if (string.IsNullOrEmpty(id))
                id = $"scene_{gameObject.GetInstanceID()}";
        }

    }

}