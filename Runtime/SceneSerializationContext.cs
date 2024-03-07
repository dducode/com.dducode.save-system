using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Cryptography;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.Internal.Templates;
using UnityEngine;
using Logger = SaveSystem.Internal.Logger;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystem {

    public sealed class SceneSerializationContext : MonoBehaviour {

        [SerializeField]
        private bool encrypt;

        [SerializeField]
        private EncryptionSettings encryptionSettings;


        public DataBuffer DataBuffer {
            get {
                if (!m_loaded)
                    Logger.LogWarning(name, Messages.TryingToReadNotLoadedData);
                return m_dataBuffer;
            }
        }

        public bool Encrypt {
            get => encrypt;
            set {
                encrypt = value;
                Logger.Log(name, $"{(value ? "Enable" : "Disable")} encryption", this);
            }
        }

        public Cryptographer Cryptographer {
            get => m_cryptographer;
            set {
                m_cryptographer = value ?? throw new ArgumentNullException(nameof(Cryptographer));
                Logger.Log(name, $"Set cryptographer: {value}");
            }
        }

        private int ObjectsCount => m_serializables.Count + m_asyncSerializables.Count;
        private string SceneName => gameObject.scene.name;

        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly List<IAsyncRuntimeSerializable> m_asyncSerializables = new();

        private DataBuffer m_dataBuffer = new();
        private bool m_loaded;
        private bool m_registrationClosed;
        private Cryptographer m_cryptographer;


        private void Awake () {
            m_cryptographer ??= new Cryptographer(encryptionSettings);
        }


        public SceneSerializationContext RegisterSerializable ([NotNull] IRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(name, Messages.RegistrationClosed, this);
                return this;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(name, $"Serializable object {serializable} was registered", this);
            return this;
        }


        public SceneSerializationContext RegisterSerializable ([NotNull] IAsyncRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(name, Messages.RegistrationClosed, this);
                return this;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_asyncSerializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(name, $"Serializable object {serializable} was registered", this);
            return this;
        }


        public SceneSerializationContext RegisterSerializables (
            [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            if (m_registrationClosed) {
                Logger.LogError(name, Messages.RegistrationClosed, this);
                return this;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();
            m_serializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log(name, $"Serializable objects was registered", this);
            return this;
        }


        public SceneSerializationContext RegisterSerializables (
            [NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables
        ) {
            if (m_registrationClosed) {
                Logger.LogError(name, Messages.RegistrationClosed, this);
                return this;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IAsyncRuntimeSerializable[] array = serializables as IAsyncRuntimeSerializable[] ?? serializables.ToArray();
            m_asyncSerializables.AddRange(array);
            DiagnosticService.AddObjects(array);
            Logger.Log(name, $"Serializable objects was registered", this);
            return this;
        }


        public async void LoadSceneData (
            Action<HandlingResult> continuation, CancellationToken token = default
        ) {
            try {
                continuation(await LoadSceneData(token));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        public async UniTask<HandlingResult> LoadSceneData (CancellationToken token = default) {
            if (m_loaded) {
                Logger.LogWarning(name, "All objects already loaded", this);
                return HandlingResult.Canceled;
            }

            string dataPath = GetPathFromProfile();

            if (!File.Exists(dataPath)) {
                m_registrationClosed = true;
                SetDefaults();
                m_loaded = true;
                return HandlingResult.FileNotExists;
            }

            m_registrationClosed = true;

            try {
                return await TryLoadSceneData(token, dataPath);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, $"{SceneName} data loading was canceled", this);
                return HandlingResult.Canceled;
            }
        }


        internal async UniTask SaveSceneData (CancellationToken token) {
            if (ObjectsCount == 0 && m_dataBuffer.Count == 0)
                return;
            if (!m_loaded)
                Logger.LogWarning(name, "Start saving when data not loaded", this);

            m_registrationClosed = true;

            token.ThrowIfCancellationRequested();
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);

            writer.Write(m_dataBuffer);
            await SerializeObjects(writer, token);

            byte[] data = memoryStream.ToArray();

            if (Encrypt)
                data = await m_cryptographer.Encrypt(data, token);

            await File.WriteAllBytesAsync(GetPathFromProfile(), data, token);
            Logger.Log(name, $"{SceneName} data saved");
        }


        private async UniTask<HandlingResult> TryLoadSceneData (CancellationToken token, string dataPath) {
            token.ThrowIfCancellationRequested();
            byte[] data = await File.ReadAllBytesAsync(dataPath, token);

            if (Encrypt)
                data = await m_cryptographer.Decrypt(data, token);

            var memoryStream = new MemoryStream(data);
            await using var reader = new SaveReader(memoryStream);

            m_dataBuffer = reader.ReadDataBuffer();
            await DeserializeObjects(reader, token);

            Logger.Log(name, $"{SceneName} data loaded", this);
            m_loaded = true;
            return HandlingResult.Success;
        }


        private async UniTask SerializeObjects (SaveWriter writer, CancellationToken token) {
            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Serialize(writer);

            foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                await serializable.Serialize(writer, token);
        }


        private async UniTask DeserializeObjects (SaveReader reader, CancellationToken token) {
            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Deserialize(reader);

            foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                await serializable.Deserialize(reader, token);
        }


        private void SetDefaults () {
            foreach (IDefault serializable in m_serializables.Select(serializable => serializable as IDefault))
                serializable?.SetDefaults();

            foreach (IDefault serializable in m_asyncSerializables.Select(serializable => serializable as IDefault))
                serializable?.SetDefaults();
        }


        private string GetPathFromProfile () {
            return Path.Combine(SaveSystemCore.SelectedSaveProfile.ProfileDataFolder, $"{SceneName}.scenedata");
        }

    }

}