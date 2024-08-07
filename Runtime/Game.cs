﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Security;
using File = SaveSystemPackage.Internal.File;

// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage {

    public class Game {

        /// <summary>
        /// Uses for serializing data into a separately file
        /// </summary>
        [NotNull]
        public SaveProfile SaveProfile {
            get => m_saveProfile;
            set {
                m_saveProfile = value ?? throw new ArgumentNullException(nameof(SaveProfile));
                if (m_sceneContext != null)
                    m_saveProfile.SceneContext = m_sceneContext;
            }
        }

        public SerializationSettings OverriddenSettings => GameScope.OverriddenSettings;
        public DataBuffer Data => GameScope.Data;
        public SecureDataBuffer SecureData => GameScope.SecureData;

        [NotNull]
        internal File DataFile {
            get => GameScope.DataFile;
            private set => GameScope.DataFile = value;
        }

        internal SceneSerializationContext SceneContext {
            get => m_sceneContext;
            set {
                if (value == null)
                    throw new ArgumentNullException(nameof(SceneContext));
                m_sceneContext = value;
            }
        }

        internal bool HasChanges => Data.HasChanges;
        private SerializationScope GameScope { get; }

        private SaveProfile m_saveProfile;
        private SceneSerializationContext m_sceneContext;


        internal Game (SaveSystemSettings settings) {
            GameScope = new SerializationScope {
                Name = "Game scope"
            };

            DataFile = Storage.Root.GetOrCreateFile(settings.dataFileName, "data");
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,IRuntimeSerializable)"/>
        public Game RegisterSerializable ([NotNull] string key, [NotNull] IRuntimeSerializable serializable) {
            GameScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,object)"/>
        public Game RegisterSerializable ([NotNull] string key, [NotNull] object obj) {
            GameScope.RegisterSerializable(key, obj);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables"/>
        public Game RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            GameScope.RegisterSerializables(key, serializables);
            return this;
        }


        /// <summary>
        /// Start saving immediately and wait it
        /// </summary>
        public async Task Save () {
            await Save(SaveSystem.exitCancellation.Token);
        }


        /// <summary>
        /// Start loading and wait it
        /// </summary>
        public async Task Load () {
            CancellationToken token = SaveSystem.exitCancellation.Token;

            try {
                token.ThrowIfCancellationRequested();
                await GameScope.Deserialize(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(GameScope.Name, "Data loading canceled");
            }
        }


        internal async Task Save (CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                await GameScope.Serialize(token);
                if (SaveProfile != null)
                    await SaveProfile.Save(token);
                else if (SceneContext != null)
                    await SceneContext.Save(token);
            }
            catch (OperationCanceledException) {
                Logger.Log(GameScope.Name, "Data saving canceled");
            }
        }


        internal async Task<StorageData> ExportGameData (CancellationToken token) {
            return DataFile.Exists
                ? new StorageData(await DataFile.ReadAllBytesAsync(token), DataFile.Name)
                : null;
        }


        internal async Task ImportGameData (byte[] data, CancellationToken token) {
            if (data.Length > 0)
                await DataFile.WriteAllBytesAsync(data, token);
        }

    }

}