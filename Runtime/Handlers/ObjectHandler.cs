using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Object Handler can help you to saving/loading <see cref="IPersistentObject">persistent objects</see>.
    /// Also you can set parameters for this as a chain of methods
    /// </summary>
    public class ObjectHandler {

        private readonly string m_filePartPath;
        private readonly IPersistentObject[] m_objects;

        private AsyncMode m_asyncMode;
        private IProgress<float> m_savingProgress;
        private IProgress<float> m_loadingProgress;
        private CancellationTokenSource m_source;
        private Action<bool> m_onComplete;
        private StorageLocation m_storageLocation;


        internal ObjectHandler (string filePartPath, IPersistentObject[] objects) {
            m_filePartPath = filePartPath;
            m_objects = objects;
        }


        /// <summary>
        /// Reset all parameters to default values
        /// </summary>
        public ObjectHandler ResetToDefault () {
            m_savingProgress = null;
            m_onComplete = null;
            m_source = null;
            m_asyncMode = AsyncMode.OnPlayerLoop;
            m_storageLocation = StorageLocation.Local;
            return this;
        }


        /// <summary>
        /// Call it to handle data on player loop
        /// </summary>
        public ObjectHandler OnPlayerLoop () {
            m_asyncMode = AsyncMode.OnPlayerLoop;
            return this;
        }


        /// <summary>
        /// Call it to handle data on thread pool
        /// </summary>
        public ObjectHandler OnThreadPool () {
            m_asyncMode = AsyncMode.OnThreadPool;
            return this;
        }


        /// <summary>
        /// You can hand over IProgress object to observe the progress of data handling
        /// </summary>
        /// <param name="progress"> Object that observes of handling progress </param>
        public ObjectHandler ObserveProgress (IProgress<float> progress) {
            m_savingProgress = progress;
            m_loadingProgress = progress;
            return this;
        }


        /// <summary>
        /// You can hand over two IProgress objects to observe the progress of loading and saving data separately
        /// </summary>
        /// <param name="savingProgress"> Object that observes of saving progress </param>
        /// <param name="loadingProgress"> Object that observes of loading progress </param>
        public ObjectHandler ObserveProgress (IProgress<float> savingProgress, IProgress<float> loadingProgress) {
            m_savingProgress = savingProgress;
            m_loadingProgress = loadingProgress;
            return this;
        }


        /// <summary>
        /// Set cancellation token source to cancel handling
        /// </summary>
        public ObjectHandler SetCancellationSource (CancellationTokenSource source) {
            m_source = source;
            return this;
        }


        /// <summary>
        /// Call it to inform the data handler what a storage location persists on a remote server
        /// </summary>
        public ObjectHandler Remote () {
            m_storageLocation = StorageLocation.Remote;
            return this;
        }


        /// <summary>
        /// Call it to inform the data handler what a storage location persists on a local machine
        /// </summary>
        public ObjectHandler Local () {
            m_storageLocation = StorageLocation.Local;
            return this;
        }


        /// <summary>
        /// Set a callback to receive a success message
        /// </summary>
        public ObjectHandler OnComplete (Action<bool> onComplete) {
            m_onComplete = onComplete;
            return this;
        }


        /// <summary>
        /// Call it to start an objects saving
        /// </summary>
        public void Save () {
            using UnityWriter unityWriter = UnityWriter.GetLocal(m_filePartPath);

            foreach (IPersistentObject obj in m_objects)
                obj.Save(unityWriter);

            m_onComplete?.Invoke(true);
        }


        /// <summary>
        /// Call it to start an objects loading
        /// </summary>
        /// <returns> True if the loading was successful, otherwise false </returns>
        public bool Load () {
            using UnityReader unityReader = UnityReader.GetLocal(m_filePartPath);

            if (unityReader is null) {
                m_onComplete?.Invoke(false);
                return false;
            }
            else {
                foreach (IPersistentObject obj in m_objects)
                    obj.Load(unityReader);

                m_onComplete?.Invoke(true);
                return true;
            }
        }


        /// <summary>
        /// Call it to start an async objects saving
        /// </summary>
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


        /// <summary>
        /// Call it to start an async objects loading
        /// </summary>
        /// <returns> True if the loading was successful, otherwise false </returns>
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
            await using UnityWriter unityWriter = UnityWriter.GetLocal(m_filePartPath);

            bool wasSuccessful = await InternalHandling.TryHandleObjectsAsync(
                m_objects, m_asyncMode, unityWriter, m_savingProgress, m_source);

            m_onComplete?.Invoke(wasSuccessful);
        }


        private async UniTask SaveRemoteHandle () {
            await using UnityWriter unityWriter = UnityWriter.GetRemote();

            bool wasSuccessful = await InternalHandling.TryHandleObjectsAsync(
                m_objects, m_asyncMode, unityWriter, m_savingProgress, m_source);

            if (wasSuccessful) {
                await unityWriter.DisposeAsync();
                await Storage.SendDataToRemote(m_filePartPath);
            }

            m_onComplete?.Invoke(wasSuccessful);
        }


        private async UniTask<bool> LoadLocalHandle () {
            using UnityReader unityReader = UnityReader.GetLocal(m_filePartPath);
            return await LoadHandle(unityReader);
        }


        private async UniTask<bool> LoadRemoteHandle () {
            using UnityReader unityReader = await UnityReader.GetRemote(m_filePartPath);
            return await LoadHandle(unityReader);
        }


        private async UniTask<bool> LoadHandle (UnityReader unityReader) {
            if (unityReader is null) {
                m_onComplete?.Invoke(false);
                return false;
            }
            else {
                if (await InternalHandling.TryHandleObjectsAsync(
                    m_objects, m_asyncMode, unityReader, m_loadingProgress, m_source)) {
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