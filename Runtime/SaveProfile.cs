﻿using System;
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

// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem {

    public class SaveProfile : IRuntimeSerializable {

        public static SaveProfile Default {
            get {
                return m_default ??= new SaveProfile {
                    Name = "default_profile", ProfileDataFolder = "default_profile"
                };
            }
        }

        private static SaveProfile m_default;

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

        public bool Encrypt {
            get => m_encrypt;
            set {
                m_encrypt = value;
                Logger.Log(Name, $"{(value ? "Enable" : "Disable")} encryption");
            }
        }

        [NotNull]
        public Cryptographer Cryptographer {
            get => m_cryptographer;
            set {
                m_cryptographer = value ?? throw new ArgumentNullException(nameof(Cryptographer));
                Logger.Log(Name, $"Set cryptographer: {value}");
            }
        }

        public DataBuffer DataBuffer {
            get {
                if (!m_loaded)
                    Logger.LogWarning(Name, Messages.TryingToReadNotLoadedData);
                return m_dataBuffer;
            }
        }

        public string DataPath => Path.Combine(m_profileDataFolder, $"{m_name}.profiledata");

        private int ObjectsCount => m_serializables.Count + m_asyncSerializables.Count;

        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly List<IAsyncRuntimeSerializable> m_asyncSerializables = new();

        private Cryptographer m_cryptographer;
        private DataBuffer m_dataBuffer = new();
        private string m_name;
        private string m_profileDataFolder;
        private bool m_loaded;
        private bool m_registrationClosed;
        private bool m_encrypt;


        public void RegisterSerializable ([NotNull] IRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(Name, $"Serializable {serializable} was registered in {Name}");
        }


        public void RegisterSerializable ([NotNull] IAsyncRuntimeSerializable serializable) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_asyncSerializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log(Name, $"Serializable {serializable} was registered in {Name}");
        }


        public void RegisterSerializables ([NotNull] IEnumerable<IRuntimeSerializable> serializables) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();
            m_serializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log(Name, $"Serializable objects were registered in {Name}");
        }


        public void RegisterSerializables ([NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables) {
            if (m_registrationClosed) {
                Logger.LogError(Name, Messages.RegistrationClosed);
                return;
            }

            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IAsyncRuntimeSerializable[] objects =
                serializables as IAsyncRuntimeSerializable[] ?? serializables.ToArray();
            m_asyncSerializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log(Name, $"Serializable objects were registered in {Name}");
        }


        public async void LoadProfileData (
            Action<HandlingResult> continuation, CancellationToken token = default
        ) {
            try {
                continuation(await LoadProfileData(token));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        public async UniTask<HandlingResult> LoadProfileData (CancellationToken token = default) {
            if (m_loaded) {
                Logger.LogWarning(Name, "All objects already loaded");
                return HandlingResult.Canceled;
            }

            if (!File.Exists(DataPath)) {
                m_registrationClosed = true;
                SetDefaults();
                m_loaded = true;
                return HandlingResult.FileNotExists;
            }

            m_registrationClosed = true;

            try {
                return await TryLoadProfileData(token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(Name, "Profile loading was canceled");
                return HandlingResult.Canceled;
            }
        }


        public virtual void Serialize (SaveWriter writer) {
            writer.Write(Name);
            writer.Write(ProfileDataFolder);
        }


        public virtual void Deserialize (SaveReader reader) {
            Name = reader.ReadString();
            ProfileDataFolder = reader.ReadString();
        }


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.PersistentDataPath, m_profileDataFolder)}";
        }


        internal async UniTask SaveProfileData (CancellationToken token) {
            if (ObjectsCount == 0 && m_dataBuffer.Count == 0)
                return;
            if (!m_loaded)
                Logger.LogWarning(Name, "Start saving when data not loaded");

            m_registrationClosed = true;

            token.ThrowIfCancellationRequested();
            var memoryStream = new MemoryStream();
            await using var writer = new SaveWriter(memoryStream);

            writer.Write(m_dataBuffer);
            await SerializeObjects(writer, token);

            byte[] data = memoryStream.ToArray();

            if (Encrypt)
                data = await m_cryptographer.Encrypt(data, token);

            await File.WriteAllBytesAsync(DataPath, data, token);
            Logger.Log(Name, "Profile saved");
        }


        private async UniTask<HandlingResult> TryLoadProfileData (CancellationToken token) {
            token.ThrowIfCancellationRequested();
            byte[] data = await File.ReadAllBytesAsync(DataPath, token);

            if (Encrypt)
                data = await m_cryptographer.Decrypt(data, token);

            var memoryStream = new MemoryStream(data);
            await using var reader = new SaveReader(memoryStream);

            m_dataBuffer = reader.ReadDataBuffer();
            await DeserializeObjects(reader, token);

            Logger.Log(Name, "Profile loaded");
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

    }

}