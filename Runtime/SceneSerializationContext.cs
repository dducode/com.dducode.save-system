using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.Internal.Extensions;
using SaveSystem.Internal.Templates;
using UnityEngine;
using Logger = SaveSystem.Internal.Logger;

// ReSharper disable UnusedMember.Global

namespace SaveSystem {

    public sealed class SceneSerializationContext : MonoBehaviour {

        public DataBuffer DataBuffer {
            get {
                if (!m_loaded)
                    Logger.LogWarning(name, Messages.TryingToReadNotLoadedData);
                return m_dataBuffer;
            }
        }

        private int ObjectsCount => m_serializables.Count + m_asyncSerializables.Count;
        private string SceneName => gameObject.scene.name;

        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly List<IAsyncRuntimeSerializable> m_asyncSerializables = new();

        private DataBuffer m_dataBuffer = new();
        private bool m_loaded;
        private bool m_registrationClosed;


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


        public async void LoadSceneDataAsync (
            Action<HandlingResult> continuation, CancellationToken token = default
        ) {
            try {
                continuation(await LoadSceneDataAsync(token));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        public async UniTask<HandlingResult> LoadSceneDataAsync (CancellationToken token = default) {
            if (m_loaded) {
                Logger.LogWarning(name, "All objects already loaded", this);
                return HandlingResult.Canceled;
            }

            string dataPath = GetPathFromProfile();

            if (!File.Exists(dataPath)) {
                m_registrationClosed = m_loaded = true;
                return HandlingResult.FileNotExists;
            }

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                var memoryStream = new MemoryStream();
                await memoryStream.ReadDataFromFileAsync(dataPath, token);
                await using var reader = new SaveReader(memoryStream);
                m_dataBuffer = reader.ReadDataBuffer();

                foreach (IRuntimeSerializable serializable in m_serializables)
                    serializable.Deserialize(reader);

                foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                    await serializable.Deserialize(reader, token);

                Logger.Log(name, $"{SceneName} data loaded", this);
                m_loaded = true;
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, $"{SceneName} data loading was canceled", this);
                return HandlingResult.Canceled;
            }
        }


        internal async UniTask SaveSceneDataAsync (CancellationToken token) {
            if (ObjectsCount == 0 && m_dataBuffer.Count == 0)
                return;
            if (!m_loaded)
                Logger.LogWarning(name, "Start saving when data not loaded", this);

            m_registrationClosed = true;

            token.ThrowIfCancellationRequested();
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);
            writer.Write(m_dataBuffer);

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Serialize(writer);

            foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                await serializable.Serialize(writer, token);

            await File.WriteAllBytesAsync(GetPathFromProfile(), ((MemoryStream)writer.input).ToArray(), token);
                
            Logger.Log(name, $"{SceneName} data saved");
        }


        private string GetPathFromProfile () {
            return Path.Combine(SaveSystemCore.SelectedSaveProfile.ProfileDataFolder, $"{SceneName}.scenedata");
        }

    }

}