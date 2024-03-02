using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SaveSystem.Exceptions;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using UnityEngine;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Logger = SaveSystem.Internal.Logger;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem {

    public class SaveProfile : IRuntimeSerializable {

        [NotNull]
        public string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_name = value;
            }
        }

        [NotNull]
        public string ProfileDataFolder {
            get => m_profileDataFolder;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_profileDataFolder = Storage.PrepareBeforeUsing(value, true);
            }
        }

        public string DataPath => Path.Combine(m_profileDataFolder, $"{m_name}.profiledata");

        private int ObjectsCount => m_serializables.Count + m_asyncSerializables.Count;

        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly List<IAsyncRuntimeSerializable> m_asyncSerializables = new();

        private DataBuffer m_buffer = new();
        private string m_name;
        private string m_profileDataFolder;
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


        public void RegisterSerializable ([NotNull] IRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(MessageTemplates.RegistrationClosedMessage);
                return;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log($"Serializable {serializable} was registered in {Name}");
        }


        public void RegisterSerializable ([NotNull] IAsyncRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(MessageTemplates.RegistrationClosedMessage);
                return;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_asyncSerializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log($"Serializable {serializable} was registered in {Name}");
        }


        public void RegisterSerializables ([NotNull] IEnumerable<IRuntimeSerializable> serializables) {
            if (m_registrationClosed) {
                Logger.LogError(MessageTemplates.RegistrationClosedMessage);
                return;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();
            m_serializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log($"Serializable objects were registered in {Name}");
        }


        public void RegisterSerializables ([NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables) {
            if (m_registrationClosed) {
                Logger.LogError(MessageTemplates.RegistrationClosedMessage);
                return;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IAsyncRuntimeSerializable[] objects =
                serializables as IAsyncRuntimeSerializable[] ?? serializables.ToArray();
            m_asyncSerializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log($"Serializable objects were registered in {Name}");
        }


        public async void LoadProfileDataAsync (
            Action<HandlingResult> continuation, CancellationToken token = default
        ) {
            try {
                continuation(await LoadProfileDataAsync(token));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        public async UniTask<HandlingResult> LoadProfileDataAsync (CancellationToken token = default) {
            if (m_loaded) {
                Logger.LogWarning("All objects already loaded");
                return HandlingResult.Canceled;
            }

            if (!File.Exists(DataPath)) {
                m_registrationClosed = m_loaded = true;
                return HandlingResult.FileNotExists;
            }

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(DataPath, token);
                m_buffer = reader.ReadDataBuffer();

                foreach (IRuntimeSerializable serializable in m_serializables)
                    serializable.Deserialize(reader);

                foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                    await serializable.Deserialize(reader, token);

                Logger.Log("Profile was loading");
                m_loaded = true;
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Profile loading was canceled");
                return HandlingResult.Canceled;
            }
        }


        public virtual void Serialize (BinaryWriter writer) {
            writer.Write(Name);
            writer.Write(ProfileDataFolder);
        }


        public virtual void Deserialize (BinaryReader reader) {
            Name = reader.ReadString();
            ProfileDataFolder = reader.ReadString();
        }


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.PersistentDataPath, m_profileDataFolder)}";
        }


        internal async UniTask SaveProfileDataAsync (CancellationToken token) {
            if (ObjectsCount == 0 && m_buffer.Count == 0)
                return;
            if (!m_loaded)
                Logger.LogWarning("Start saving when data not loaded");

            m_registrationClosed = true;

            try {
                token.ThrowIfCancellationRequested();
                using var writer = new BinaryWriter(new MemoryStream());
                writer.Write(m_buffer);

                foreach (IRuntimeSerializable serializable in m_serializables)
                    serializable.Serialize(writer);

                foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                    await serializable.Serialize(writer, token);

                await writer.WriteDataToFileAsync(DataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Profile saving was canceled");
            }
        }

    }

}