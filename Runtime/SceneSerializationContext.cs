using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Exceptions;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using UnityEngine;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Logger = SaveSystem.Internal.Logger;

// ReSharper disable UnusedMember.Global

namespace SaveSystem {

    public sealed class SceneSerializationContext : MonoBehaviour {

        [HideInInspector]
        public string sceneName;

        private int ObjectsCount => m_serializables.Count + m_asyncSerializables.Count;
        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly List<IAsyncRuntimeSerializable> m_asyncSerializables = new();

        private DataBuffer m_buffer = new();
        private bool m_loaded;
        private bool m_registrationClosed;


        /// <summary>
        /// Write any data for saving
        /// </summary>
        public void WriteData<TValue> (string key, TValue value) where TValue : unmanaged {
            m_buffer.Add(key, value);
        }


        /// <summary>
        /// Get writable data
        /// </summary>
        public TValue ReadData<TValue> (string key) where TValue : unmanaged {
            if (!m_loaded)
                throw new DataNotLoadedException(MessageTemplates.CannotReadDataMessage);

            return m_buffer.Get<TValue>(key);
        }


        public SceneSerializationContext RegisterSerializable ([NotNull] IRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(MessageTemplates.RegistrationClosedMessage);
                return this;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log($"Serializable object {serializable} was registered in {name}", this);
            return this;
        }


        public SceneSerializationContext RegisterSerializable ([NotNull] IAsyncRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(MessageTemplates.RegistrationClosedMessage);
                return this;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_asyncSerializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log($"Serializable object {serializable} was registered in {name}", this);
            return this;
        }


        public SceneSerializationContext RegisterSerializables (
            [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            if (m_registrationClosed) {
                Logger.LogError(MessageTemplates.RegistrationClosedMessage);
                return this;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();
            m_serializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log($"Serializable objects was registered in {name}", this);
            return this;
        }


        public SceneSerializationContext RegisterSerializables (
            [NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables
        ) {
            if (m_registrationClosed) {
                Logger.LogError(MessageTemplates.RegistrationClosedMessage);
                return this;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IAsyncRuntimeSerializable[] array = serializables as IAsyncRuntimeSerializable[] ?? serializables.ToArray();
            m_asyncSerializables.AddRange(array);
            DiagnosticService.AddObjects(array);
            Logger.Log($"Serializable objects was registered in {name}", this);
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
                Logger.LogWarning("All objects already loaded", this);
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
                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(dataPath, token);
                m_buffer = reader.ReadDataBuffer();

                foreach (IRuntimeSerializable serializable in m_serializables)
                    serializable.Deserialize(reader);

                foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                    await serializable.Deserialize(reader, token);

                Logger.Log($"{sceneName} data was loaded", this);
                m_loaded = true;
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning($"{sceneName} data loading was canceled", this);
                return HandlingResult.Canceled;
            }
        }


        internal async UniTask SaveSceneDataAsync (CancellationToken token) {
            if (ObjectsCount == 0 && m_buffer.Count == 0)
                return;
            if (!m_loaded)
                Logger.LogWarning("Start saving when data not loaded", this);

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                using var writer = new BinaryWriter(new MemoryStream());
                writer.Write(m_buffer);

                foreach (IRuntimeSerializable serializable in m_serializables)
                    serializable.Serialize(writer);

                foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                    await serializable.Serialize(writer, token);

                await writer.WriteDataToFileAsync(GetPathFromProfile(), token);
                Logger.Log($"{sceneName} data was saved");
            }
            catch (OperationCanceledException) {
                Logger.LogWarning($"{sceneName} data saving was canceled", this);
            }
        }


        private string GetPathFromProfile () {
            return Path.Combine(SaveSystemCore.SelectedSaveProfile.ProfileDataFolder, $"{sceneName}.scenedata");
        }

    }

}