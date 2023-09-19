using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// It's same as <see cref="ObjectHandler"/> only for the IPersistentObjectAsync
    /// </summary>
    public class AdvancedObjectHandler {

        private readonly string m_path;
        private readonly IPersistentObjectAsync[] m_objects;
        private IProgress<float> m_savingProgress;
        private IProgress<float> m_loadingProgress;
        private CancellationTokenSource m_source;
        private Action<bool> m_onComplete;
        private StorageLocation m_storageLocation;


        internal AdvancedObjectHandler (string path, IPersistentObjectAsync[] objects) {
            m_path = path;
            m_objects = objects;
            m_storageLocation = StorageLocation.Local;
        }


        /// <inheritdoc cref="ObjectHandler.ResetToDefault"/>
        public AdvancedObjectHandler ResetToDefault () {
            m_savingProgress = null;
            m_onComplete = null;
            m_source = null;
            m_storageLocation = StorageLocation.Local;
            return this;
        }


        /// <inheritdoc cref="ObjectHandler.ObserveProgress(IProgress{float})"/>
        public AdvancedObjectHandler ObserveProgress (IProgress<float> progress) {
            m_savingProgress = progress;
            m_loadingProgress = progress;
            return this;
        }


        /// <inheritdoc cref="ObjectHandler.ObserveProgress(IProgress{float}, IProgress{float})"/>
        public AdvancedObjectHandler ObserveProgress (
            IProgress<float> savingProgress,
            IProgress<float> loadingProgress
        ) {
            m_savingProgress = savingProgress;
            m_loadingProgress = loadingProgress;
            return this;
        }


        /// <inheritdoc cref="ObjectHandler.SetCancellationSource"/>
        public AdvancedObjectHandler SetCancellationSource (CancellationTokenSource source) {
            m_source = source;
            return this;
        }


        /// <inheritdoc cref="ObjectHandler.Remote"/>
        public AdvancedObjectHandler Remote () {
            m_storageLocation = StorageLocation.Remote;
            return this;
        }


        /// <inheritdoc cref="ObjectHandler.Local"/>
        public AdvancedObjectHandler Local () {
            m_storageLocation = StorageLocation.Local;
            return this;
        }


        /// <inheritdoc cref="ObjectHandler.OnComplete"/>
        public AdvancedObjectHandler OnComplete (Action<bool> onComplete) {
            m_onComplete = onComplete;
            return this;
        }


        /// <inheritdoc cref="ObjectHandler.SaveAsync"/>
        public async UniTask SaveAsync () {
            switch (m_storageLocation) {
                case StorageLocation.Local:
                    await SaveLocalHandle();
                    break;
                case StorageLocation.Remote:
                    await SaveRemoteHandle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <inheritdoc cref="ObjectHandler.LoadAsync"/>
        public async UniTask<bool> LoadAsync () {
            switch (m_storageLocation) {
                case StorageLocation.Local:
                    return await LoadLocalHandle();
                case StorageLocation.Remote:
                    return await LoadRemoteHandle();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private async UniTask SaveLocalHandle () {
            await using UnityAsyncWriter unityAsyncWriter = UnityAsyncWriter.GetLocal(m_path);

            bool wasSuccessful = await InternalHandling.Advanced.TryHandleObjectsAsync(
                m_objects, unityAsyncWriter, m_savingProgress, m_source);

            m_onComplete?.Invoke(wasSuccessful);
        }


        private async UniTask SaveRemoteHandle () {
            await using UnityAsyncWriter unityAsyncWriter = UnityAsyncWriter.GetRemote();

            bool wasSuccessful = await InternalHandling.Advanced.TryHandleObjectsAsync(
                m_objects, unityAsyncWriter, m_savingProgress, m_source);

            await unityAsyncWriter.DisposeAsync();
            await Storage.SendDataToRemote(m_path);
            m_onComplete?.Invoke(wasSuccessful);
        }


        private async UniTask<bool> LoadLocalHandle () {
            using UnityAsyncReader unityAsyncReader = UnityAsyncReader.GetLocal(m_path);
            return await LoadHandle(unityAsyncReader);
        }


        private async UniTask<bool> LoadRemoteHandle () {
            using UnityAsyncReader unityAsyncReader = await UnityAsyncReader.GetRemote(m_path);
            return await LoadHandle(unityAsyncReader);
        }


        private async UniTask<bool> LoadHandle (UnityAsyncReader unityAsyncReader) {
            if (unityAsyncReader is null) {
                m_onComplete?.Invoke(false);
                return false;
            }
            else {
                if (await InternalHandling.Advanced.TryHandleObjectsAsync(
                    m_objects, unityAsyncReader, m_loadingProgress, m_source)) {
                    m_onComplete?.Invoke(true);
                    return true;
                }
                else {
                    m_onComplete?.Invoke(false);
                    return false;
                }
            }
        }

    }

}