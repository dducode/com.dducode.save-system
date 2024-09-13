using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.ComponentsRecording;
using SaveSystemPackage.Compressing;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Security;
using UnityEngine;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    [AddComponentMenu("Save System/Scene Serialization Context")]
    [DisallowMultipleComponent]
    public sealed class SceneSerializationContext : MonoBehaviour {

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

        public DataBuffer Data => SceneScope.Data;
        public SecureDataBuffer SecureData => SceneScope.SecureData;

        internal bool HasChanges => Data.HasChanges || SecureData.HasChanges;
        private SerializationScope SceneScope { get; set; }
        private SerializationSettings OverriddenSettings => SceneScope.OverriddenSettings;

        private Internal.File DataFile {
            get => SceneScope.DataFile;
            set => SceneScope.DataFile = value;
        }


        private void Awake () {
            SceneScope = new SerializationScope {
                Name = $"{gameObject.scene.name} scene scope"
            };

            if (overrideProjectSettings) {
                OverriddenSettings.Encrypt = encrypt;

                if (encrypt) {
                    OverriddenSettings.Cryptographer = encryptionSettings.useCustomCryptographer
                        ? encryptionSettings.reference
                        : new Cryptographer(encryptionSettings);
                }

                OverriddenSettings.CompressFiles = compressFiles;

                if (compressFiles) {
                    OverriddenSettings.FileCompressor = compressionSettings.useCustomCompressor
                        ? compressionSettings.reference
                        : new FileCompressor(compressionSettings);
                }
            }

            SaveProfile profile = SaveSystem.Game.SaveProfile;

            if (profile == null) {
                SaveSystem.Game.SceneContext = this;
                DataFile = Storage.ScenesDirectory.GetOrCreateFile(fileName, "scenedata");
            }
            else {
                profile.SceneContext = this;
                DataFile = profile.DataDirectory.GetOrCreateFile(fileName, "scenedata");
            }

            RegisterRecorders();
        }


        private void OnValidate () {
            if (string.IsNullOrEmpty(fileName))
                fileName = gameObject.scene.name.ToPathFormat();
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,IRuntimeSerializable)"/>
        public SceneSerializationContext RegisterSerializable (
            [NotNull] string key, [NotNull] IRuntimeSerializable serializable
        ) {
            SceneScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,object)"/>
        public SceneSerializationContext RegisterSerializable ([NotNull] string key, [NotNull] object obj) {
            SceneScope.RegisterSerializable(key, obj);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables"/>
        public SceneSerializationContext RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            SceneScope.RegisterSerializables(key, serializables);
            return this;
        }


        public async Task Save () {
            await Save(SaveSystem.exitCancellation.Token);
        }


        public async Task Load () {
            CancellationToken token = SaveSystem.exitCancellation.Token;

            try {
                token.ThrowIfCancellationRequested();
                await SceneScope.Deserialize(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(SceneScope.Name, "Data loading canceled");
            }
        }


        internal async Task Save (CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                await SceneScope.Serialize(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(SceneScope.Name, "Data saving canceled");
            }
        }


        internal async Task<StorageData> ExportSceneData (CancellationToken token) {
            return DataFile.Exists
                ? new StorageData(await DataFile.ReadAllBytesAsync(token), DataFile.Name)
                : null;
        }


        internal async Task ImportSceneData (byte[] data, CancellationToken token) {
            if (data.Length > 0)
                await DataFile.WriteAllBytesAsync(data, token);
        }


        internal void Clear () {
            SceneScope.Clear();
        }


        private void RegisterRecorders () {
            foreach (ComponentRecorder recorder in FindObjectsByType<ComponentRecorder>(FindObjectsSortMode.None)) {
                recorder.Initialize();
                RegisterSerializable(recorder.Id, recorder);
            }
        }

    }

}