using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.ComponentsRecording;
using SaveSystemPackage.Internal.Extensions;
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
        private bool encrypt;

        [SerializeField]
        private string fileName;

        public SerializationSettings Settings => SceneScope.Settings;
        public DataBuffer Data => SceneScope.Data;
        internal bool HasChanges => Data.HasChanges;
        private SerializationScope SceneScope { get; set; }

        private Internal.File DataFile {
            get => SceneScope.DataFile;
            set => SceneScope.DataFile = value;
        }


        private void Awake () {
            SceneScope = new SerializationScope {
                Name = $"{gameObject.scene.name} scene scope",
                Settings = {
                    Encrypt = encrypt
                }
            };

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


        public async UniTask Save (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await SceneScope.Serialize(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(SceneScope.Name, "Data saving canceled");
            }
        }


        public async UniTask Load (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await SceneScope.Deserialize(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(SceneScope.Name, "Data loading canceled");
            }
        }


        internal async UniTask<StorageData> ExportSceneData (CancellationToken token = default) {
            return DataFile.Exists
                ? new StorageData(await DataFile.ReadAllBytesAsync(token), DataFile.Name)
                : null;
        }


        internal async UniTask ImportSceneData (byte[] data, CancellationToken token = default) {
            if (data.Length > 0)
                await DataFile.WriteAllBytesAsync(data, token);
        }


        internal void Clear () {
            SceneScope.Clear();
        }


        private void RegisterRecorders () {
            foreach (ComponentRecorder recorder in FindObjectsByType<ComponentRecorder>(FindObjectsSortMode.None))
                RegisterSerializable(recorder.Id, recorder);
        }

    }

}