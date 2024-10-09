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

        public SceneSerializationContext SceneContext { get; private set; }


        private void Awake () {
            SceneContext = new SceneSerializationContext {
                Name = $"{gameObject.scene.name} scene scope",
                Serializer = SaveSystem.Settings.SharedSerializer
            };

            SaveProfile profile = SaveSystem.Game.SaveProfile;
            string fileExtension = SaveSystem.Settings.SharedSerializer.GetFormatCode();

            if (profile == null) {
                Directory directory = Storage.ScenesDirectory.GetOrCreateDirectory(id);
                SceneContext.KeyProvider = new KeyDecorator(SaveSystem.Game.KeyProvider, directory.Name);
                SceneContext.DataStorage = new FileSystemStorage(directory, fileExtension);
                SaveSystem.Game.SceneContext = SceneContext;
            }
            else {
                Directory directory = profile.directory.GetOrCreateDirectory(id);
                SceneContext.KeyProvider = new KeyDecorator(profile.KeyProvider, directory.Name);
                SceneContext.DataStorage = new FileSystemStorage(directory, fileExtension);
                profile.SceneContext = SceneContext;
            }

            onInitialized?.Invoke();
        }


        private void OnValidate () {
            if (string.IsNullOrEmpty(id))
                id = $"scene_{gameObject.GetInstanceID()}";
        }

    }

}