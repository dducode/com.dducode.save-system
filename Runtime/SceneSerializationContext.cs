﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Cryptography;
using SaveSystem.Internal;
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

        /// <summary>
        /// Cryptographer used to encrypt/decrypt serializable data
        /// </summary>
        [NotNull]
        public Cryptographer Cryptographer {
            get => m_handler.Cryptographer;
            set => m_handler.Cryptographer = value;
        }

        public string DataPath => Path.Combine(
            SaveSystemCore.SelectedSaveProfile.DataFolder, $"{gameObject.scene.name}.scenedata"
        );

        private SaveDataHandler m_handler;
        private SerializationScope m_serializationScope;


        private void Awake () {
            m_handler = new SaveDataHandler {
                SerializationScope = m_serializationScope = new SerializationScope {
                    Name = $"{name} scope"
                }
            };

            if (encrypt)
                Cryptographer = new Cryptographer(encryptionSettings);
        }


        public void WriteData<TValue> ([NotNull] string key, TValue value) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            m_serializationScope.WriteData(key, value);
        }


        [Pure]
        public TValue ReadData<TValue> ([NotNull] string key) where TValue : unmanaged {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return m_serializationScope.ReadData<TValue>(key);
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IRuntimeSerializable)"/>
        public SceneSerializationContext RegisterSerializable (
            [NotNull] string key, [NotNull] IRuntimeSerializable serializable
        ) {
            m_serializationScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializable(string,SaveSystem.IAsyncRuntimeSerializable)"/>
        public SceneSerializationContext RegisterSerializable (
            [NotNull] string key, [NotNull] IAsyncRuntimeSerializable serializable
        ) {
            m_serializationScope.RegisterSerializable(key, serializable);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IRuntimeSerializable})"/>
        public SceneSerializationContext RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IRuntimeSerializable> serializables
        ) {
            m_serializationScope.RegisterSerializables(key, serializables);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.RegisterSerializables(string,System.Collections.Generic.IEnumerable{SaveSystem.IAsyncRuntimeSerializable})"/>
        public SceneSerializationContext RegisterSerializables (
            [NotNull] string key, [NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables
        ) {
            m_serializationScope.RegisterSerializables(key, serializables);
            return this;
        }


        /// <inheritdoc cref="SerializationScope.ObserveProgress(IProgress{float})"/>
        public void ObserveProgress ([NotNull] IProgress<float> progress) {
            m_serializationScope.ObserveProgress(progress);
        }


        /// <inheritdoc cref="SerializationScope.ObserveProgress(IProgress{float}, IProgress{float})"/>
        public void ObserveProgress (
            [NotNull] IProgress<float> saveProgress, [NotNull] IProgress<float> loadProgress
        ) {
            m_serializationScope.ObserveProgress(saveProgress, loadProgress);
        }


        public async UniTask<HandlingResult> SaveSceneData (
            [NotNull] string dataPath, CancellationToken token = default
        ) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            try {
                return await m_handler.SaveData(dataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data saving canceled", this);
                return HandlingResult.Canceled;
            }
        }


        [Pure]
        public async UniTask<(HandlingResult, byte[])> SaveSceneData (CancellationToken token = default) {
            try {
                return await m_handler.SaveData(token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data saving canceled", this);
                return (HandlingResult.Canceled, Array.Empty<byte>());
            }
        }


        public async UniTask<HandlingResult> LoadSceneData (
            [NotNull] string dataPath, CancellationToken token = default
        ) {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            try {
                return await m_handler.LoadData(dataPath, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data loading canceled", this);
                return HandlingResult.Canceled;
            }
        }


        public async UniTask<HandlingResult> LoadSceneData ([NotNull] byte[] data, CancellationToken token = default) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try {
                return await m_handler.LoadData(data, token);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(name, "Scene data loading canceled", this);
                return HandlingResult.Canceled;
            }
        }

    }

}