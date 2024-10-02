using System.Threading.Tasks;
using SaveSystemPackage.ComponentsRecording;
using SaveSystemPackage.Compressing;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Security;
using UnityEngine;

namespace SaveSystemPackage {

    [AddComponentMenu("Save System/Scene Serialization Scope")]
    [DisallowMultipleComponent]
    public class SceneSerializationScopeComponent : MonoBehaviour {

        [SerializeField]
        private bool overrideProjectSettings;

        [SerializeField]
        private bool compressFiles;

        [SerializeField]
        private CompressionSettings compressionSettings;

        [SerializeField]
        private bool encrypt;

        [SerializeField]
        private EncryptionSettings encryptionSettings;

        [SerializeField]
        private string fileName;

        public SceneSerializationScope SceneScope { get; private set; }


        private async void Awake () {
            SceneScope = new SceneSerializationScope {
                Name = $"{gameObject.scene.name} scene scope"
            };

            if (overrideProjectSettings) {
                SceneScope.OverriddenSettings.Encrypt = encrypt;

                if (encrypt) {
                    SceneScope.OverriddenSettings.Cryptographer = encryptionSettings.useCustomCryptographer
                        ? encryptionSettings.reference
                        : new Cryptographer(encryptionSettings);
                }

                SceneScope.OverriddenSettings.CompressFiles = compressFiles;

                if (compressFiles) {
                    SceneScope.OverriddenSettings.FileCompressor = compressionSettings.useCustomCompressor
                        ? compressionSettings.reference
                        : new FileCompressor(compressionSettings);
                }
            }

            SaveProfile profile = SaveSystem.Game.SaveProfile;

            if (profile == null) {
                SaveSystem.Game.SceneScope = SceneScope;
                // SceneScope.DataFile = Storage.ScenesDirectory.GetOrCreateFile(fileName, "scenedata");
            }
            else {
                profile.SceneScope = SceneScope;
                // SceneScope.DataFile = profile.DataDirectory.GetOrCreateFile(fileName, "scenedata");
            }

            await RegisterRecorders();
        }


        private void OnValidate () {
            if (string.IsNullOrEmpty(fileName))
                fileName = gameObject.scene.name.ToPathFormat();
        }


        private async Task RegisterRecorders () {
            foreach (ComponentRecorder recorder in FindObjectsByType<ComponentRecorder>(FindObjectsSortMode.None))
                await recorder.Initialize(SceneScope);
        }

    }

}