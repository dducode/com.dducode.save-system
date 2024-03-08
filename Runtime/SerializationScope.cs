using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.Internal.Templates;
using Logger = SaveSystem.Internal.Logger;

// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystem {

    public sealed class SerializationScope {

        [NotNull]
        public string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                m_name = value;
            }
        }

        private string m_name;

        private DataBuffer m_dataBuffer = new();
        private readonly Dictionary<string, IRuntimeSerializable> m_serializables = new();
        private readonly Dictionary<string, IAsyncRuntimeSerializable> m_asyncSerializables = new();
        private int ObjectsCount => m_serializables.Count + m_asyncSerializables.Count;

        private static IProgress<float> m_saveProgress;
        private static IProgress<float> m_loadProgress;

        private bool m_registrationClosed;


        public void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_dataBuffer.Write(key, value);
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (m_dataBuffer.Count == 0) {
                Logger.LogWarning(Name, "Data buffer is empty, return default value");
                return default;
            }

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_dataBuffer.Get<TValue>(key);
        }


        /// <summary>
        /// Registers an serializable object to save
        /// </summary>
        public void RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(key, serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(Name, $"Serializable object {serializable} registered in {Name}");
        }


        /// <summary>
        /// Registers an async serializable object to save
        /// </summary>
        public void RegisterSerializable ([NotNull] string key, [NotNull] IAsyncRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_asyncSerializables.Add(key, serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(Name, $"Serializable object {serializable} registered in {Name}");
        }


        /// <summary>
        /// Registers some serializable objects to save
        /// </summary>
        public void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();

            for (var i = 0; i < objects.Length; i++)
                m_serializables.Add($"{key}_{i}", objects[i]);

            DiagnosticService.AddObjects(objects);
            Logger.Log(Name, $"Serializable objects was registered in {Name}");
        }


        /// <summary>
        /// Registers some async serializable objects to save
        /// </summary>
        public void RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables
        ) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IAsyncRuntimeSerializable[] objects =
                serializables as IAsyncRuntimeSerializable[] ?? serializables.ToArray();

            for (var i = 0; i < objects.Length; i++)
                m_asyncSerializables.Add($"{key}_{i}", objects[i]);

            DiagnosticService.AddObjects(objects);
            Logger.Log(Name, $"Serializable objects was registered in {Name}");
        }


        /// <summary>
        /// Pass <see cref="IProgress{T}"> IProgress</see> object to observe progress
        /// when it'll be started
        /// </summary>
        public void ObserveProgress ([NotNull] IProgress<float> progress) {
            m_saveProgress = progress ?? throw new ArgumentNullException(nameof(progress));
            m_loadProgress = progress;
            Logger.Log(Name, $"Progress observer {progress} was register");
        }


        /// <summary>
        /// Pass two <see cref="IProgress{T}"> IProgress </see> objects to observe saving and loading progress
        /// when it'll be started
        /// </summary>
        public void ObserveProgress (
            [NotNull] IProgress<float> saveProgress, [NotNull] IProgress<float> loadProgress
        ) {
            m_saveProgress = saveProgress ?? throw new ArgumentNullException(nameof(saveProgress));
            m_loadProgress = loadProgress ?? throw new ArgumentNullException(nameof(loadProgress));
            Logger.Log(Name, $"Progress observers {saveProgress} and {loadProgress} was registered");
        }


        public async UniTask<byte[]> SaveData (CancellationToken token) {
            if (ObjectsCount == 0 && m_dataBuffer.Count == 0)
                return Array.Empty<byte>();

            m_registrationClosed = true;

            token.ThrowIfCancellationRequested();
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);

            writer.Write(m_dataBuffer);
            await SerializeObjects(writer, token);

            Logger.Log(Name, "Data saved");
            return memoryStream.ToArray();
        }


        public async UniTask<HandlingResult> LoadData ([NotNull] byte[] data, CancellationToken token = default) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                var memoryStream = new MemoryStream(data);
                await using var reader = new SaveReader(memoryStream);

                m_dataBuffer = reader.ReadDataBuffer();
                await DeserializeObjects(reader, token);

                Logger.Log(Name, "Data loaded");
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        public void SetDefaults () {
            m_registrationClosed = true;

            foreach (IDefault serializable in m_serializables.Select(pair => pair.Value as IDefault))
                serializable?.SetDefaults();

            foreach (IDefault serializable in m_asyncSerializables.Select(pair => pair.Value as IDefault))
                serializable?.SetDefaults();
        }


        private async UniTask SerializeObjects (SaveWriter writer, CancellationToken token) {
            var progress = 0;

            foreach ((string key, IRuntimeSerializable serializable) in m_serializables) {
                writer.Write(key);
                serializable.Serialize(writer);
                ReportProgress(ref progress, ObjectsCount, m_saveProgress);
            }

            foreach ((string key, IAsyncRuntimeSerializable serializable) in m_asyncSerializables) {
                writer.Write(key);
                await serializable.Serialize(writer, token);
                ReportProgress(ref progress, ObjectsCount, m_saveProgress);
            }
        }


        private async UniTask DeserializeObjects (SaveReader reader, CancellationToken token) {
            var progress = 0;

            foreach (KeyValuePair<string, IRuntimeSerializable> unused in m_serializables) {
                m_serializables[reader.ReadString()].Deserialize(reader);
                ReportProgress(ref progress, ObjectsCount, m_loadProgress);
            }

            foreach (KeyValuePair<string, IAsyncRuntimeSerializable> unused in m_asyncSerializables) {
                await m_asyncSerializables[reader.ReadString()].Deserialize(reader, token);
                ReportProgress(ref progress, ObjectsCount, m_loadProgress);
            }
        }


        private static void ReportProgress (ref int completedTasks, int tasksCount, IProgress<float> progress) {
            progress?.Report((float)++completedTasks / tasksCount);
        }

    }

}