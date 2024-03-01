using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using UnityEngine;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Logger = SaveSystem.Internal.Logger;

namespace SaveSystem {

    public sealed class SceneSerializationContext : MonoBehaviour {

        [HideInInspector]
        public string sceneName;

        private int ObjectsCount => m_serializables.Count + m_asyncSerializables.Count;
        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly List<IAsyncRuntimeSerializable> m_asyncSerializables = new();
        
        private bool m_loaded;
        private bool m_registrationClosed;


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


        internal async UniTask SaveSceneDataAsync (SaveProfile context, CancellationToken token) {
            if (ObjectsCount == 0)
                return;
            if (!m_loaded)
                Logger.LogWarning("Start saving when objects not loaded", this);

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                using var writer = new BinaryWriter(new MemoryStream());

                foreach (IRuntimeSerializable serializable in m_serializables)
                    serializable.Serialize(writer);

                foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                    await serializable.Serialize(writer, token);

                await writer.WriteDataToFileAsync(GetPathFromContext(context), token);
                Logger.Log($"{sceneName} data was saved");
            }
            catch (OperationCanceledException) {
                Logger.LogWarning($"{sceneName} data saving was canceled", this);
            }
        }


        internal async UniTask<HandlingResult> LoadSceneDataAsync (SaveProfile context, CancellationToken token) {
            if (m_loaded) {
                Logger.LogWarning("All objects already loaded", this);
                return HandlingResult.Canceled;
            }

            if (ObjectsCount == 0) {
                Logger.LogError(
                    "Cannot start loading operation - scene serialization context hasn't any objects for loading",
                    this
                );
                return HandlingResult.Error;
            }

            string dataPath = GetPathFromContext(context);

            if (!File.Exists(dataPath)) {
                m_registrationClosed = m_loaded = true;
                return HandlingResult.FileNotExists;
            }

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(dataPath, token);

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


        private string GetPathFromContext (SaveProfile context) {
            return Path.Combine(context.ProfileDataFolder, $"{sceneName}.scenedata");
        }

    }

}