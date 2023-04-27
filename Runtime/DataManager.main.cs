using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SaveSystem {

    /// <summary>
    /// Main class for handling objects
    /// </summary>
    public static partial class DataManager {

        #region Saving

        /// <summary>
        /// Save one object
        /// </summary>
        /// <param name="fileName"> the file where the object data will be saved </param>
        /// <param name="obj"> the object which will be saved </param>
        public static void SaveObject<T> (string fileName, T obj) where T : IPersistentObject {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(SAVING_OPERATION));
                return;
            }

            SaveObjects(fileName, new[] {obj});
        }


        /// <summary>
        /// Save many objects
        /// </summary>
        /// <param name="fileName"> the file where the object data will be saved </param>
        /// <param name="objects"> the objects which will be saved </param>
        public static void SaveObjects<T> (string fileName, T[] objects) where T : IPersistentObject {
            if (objects is null) {
                Debug.LogError(ObjectsArrayIsNullMessage(SAVING_OPERATION));
                return;
            }

            if (objects.Length is 0) {
                Debug.LogWarning(ObjectsArrayIsEmptyMessage(SAVING_OPERATION));
                return;
            }

            using var unityWriter = GetUnityWriter(fileName);

            foreach (var obj in objects)
                obj.Save(unityWriter);
        }

        #endregion



        #region Loading

        /// <summary>
        /// Load one object
        /// </summary>
        /// <param name="fileName"> the file whereof the object data will be load </param>
        /// <param name="obj"> the object which will be load </param>
        /// <returns> Returns true if there is saved data, otherwise false </returns>
        public static bool LoadObject<T> (string fileName, T obj) where T : IPersistentObject {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(LOADING_OPERATION));
                return false;
            }

            return LoadObjects(fileName, new[] {obj});
        }


        /// <summary>
        /// Load many objects
        /// </summary>
        /// <param name="fileName"> the file whereof the object data will be load </param>
        /// <param name="objects"> the objects which will be load </param>
        /// <returns> Returns true if there is saved data, otherwise false </returns>
        public static bool LoadObjects<T> (string fileName, T[] objects) where T : IPersistentObject {
            if (objects is null) {
                Debug.LogError(ObjectsArrayIsNullMessage(LOADING_OPERATION));
                return false;
            }

            if (objects.Length is 0) {
                Debug.LogWarning(ObjectsArrayIsEmptyMessage(LOADING_OPERATION));
                return false;
            }

            using var unityReader = GetUnityReader(fileName);

            if (unityReader is null)
                return false;

            foreach (var obj in objects)
                obj.Load(unityReader);

            return true;
        }

        #endregion



        #region SavingAsync

        /// <summary>
        /// Save one object async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when saving is canceled </exception>
        /// <inheritdoc cref="SaveObject{T}"/>
        public static async UniTask SaveObjectAsync<T> (
            string fileName,
            T obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(SAVING_OPERATION));
                return;
            }

            await SaveObjectsAsync(fileName, new[] {obj}, asyncMode, null, source, onComplete);
        }


        /// <summary>
        /// Save many objects async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when saving is canceled </exception>
        /// <inheritdoc cref="SaveObjects{T}"/>
        public static async UniTask SaveObjectsAsync<T> (
            string fileName,
            T[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            if (objects is null) {
                Debug.LogError(ObjectsArrayIsNullMessage(SAVING_OPERATION));
                source?.Dispose();
                return;
            }

            if (objects.Length is 0) {
                Debug.LogWarning(ObjectsArrayIsEmptyMessage(SAVING_OPERATION));
                source?.Dispose();
                return;
            }
            
            await using var unityWriter = GetUnityWriter(fileName);

            var completedTasks = 0f;
            source ??= new CancellationTokenSource();

            try {
                switch (asyncMode) {
                    case AsyncMode.OnThreadPool:
                        await UniTask.RunOnThreadPool(() => SavingObjects(source, unityWriter));
                        break;
                    case AsyncMode.OnPlayerLoop:
                        await SavingObjects(source, unityWriter);
                        break;
                    default: {
                        throw new NotImplementedException(
                            AsyncModeIsNotImplementMessage(asyncMode, nameof(SaveObjectsAsync))
                        );
                    }
                }
                
                onComplete?.Invoke();
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                await unityWriter.DisposeAsync();
                File.Delete(unityWriter.localPath);
                Debug.LogWarning(CancelSavingMessage);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            finally {
                source.Dispose();
            }

            async UniTask SavingObjects (
                CancellationTokenSource tokenSource,
                UnityWriter writer
            ) {
                foreach (var obj in objects) {
                    if (tokenSource.IsCancellationRequested)
                        throw new OperationCanceledException();
                    obj.Save(writer);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Length);
                    if (asyncMode == AsyncMode.OnPlayerLoop)
                        await UniTask.NextFrame(tokenSource.Token);
                }
            }
        }

        #endregion



        #region LoadingAsync

        /// <summary>
        /// Load one object async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when loading is canceled </exception>
        /// <inheritdoc cref="LoadObject{T}"/>
        public static async UniTask<bool> LoadObjectAsync<T> (
            string fileName,
            T obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(LOADING_OPERATION));
                return false;
            }

            return await LoadObjectsAsync(fileName, new[] {obj}, asyncMode, null, source, onComplete);
        }


        /// <summary>
        /// Load many objects async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when loading is canceled </exception>
        /// <inheritdoc cref="LoadObjects{T}"/>
        public static async UniTask<bool> LoadObjectsAsync<T> (
            string fileName,
            T[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            if (objects is null) {
                Debug.LogError(ObjectsArrayIsNullMessage(LOADING_OPERATION));
                source?.Dispose();
                return false;
            }

            if (objects.Length is 0) {
                Debug.LogWarning(ObjectsArrayIsEmptyMessage(LOADING_OPERATION));
                source?.Dispose();
                return false;
            }

            using var unityReader = GetUnityReader(fileName);

            if (unityReader is null) {
                source?.Dispose();
                return false;
            }

            var completesTasks = 0f;
            source ??= new CancellationTokenSource();

            try {
                switch (asyncMode) {
                    case AsyncMode.OnThreadPool:
                        await UniTask.RunOnThreadPool(() => LoadingObjects(source, unityReader));
                        break;
                    case AsyncMode.OnPlayerLoop:
                        await LoadingObjects(source, unityReader);
                        break;
                    default: {
                        throw new NotImplementedException(
                            AsyncModeIsNotImplementMessage(asyncMode, nameof(LoadObjectsAsync))
                        );
                    }
                }

                onComplete?.Invoke();
                return true;
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                Debug.Log(CancelLoadingMessage);
                return false;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                return false;
            }
            finally {
                source.Dispose();
            }

            async UniTask LoadingObjects (
                CancellationTokenSource tokenSource,
                UnityReader reader
            ) {
                foreach (var obj in objects) {
                    if (tokenSource.IsCancellationRequested)
                        throw new OperationCanceledException();
                    obj.Load(reader);
                    completesTasks++;
                    progress?.Report(completesTasks / objects.Length);
                    if (asyncMode == AsyncMode.OnPlayerLoop)
                        await UniTask.NextFrame(tokenSource.Token);
                }
            }
        }

        #endregion



        #region SavingAsyncAdvanced

        /// <inheritdoc cref="SaveObjectAsync{T}"/>
        public static async UniTask SaveObjectAsyncAdvanced<T> (
            string fileName,
            T obj,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObjectAsync {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(SAVING_OPERATION));
                return;
            }

            await SaveObjectsAsyncAdvanced(fileName, new[] {obj}, null, source, onComplete);
        }


        /// <inheritdoc cref="SaveObjectsAsync{T}"/>
        public static async UniTask SaveObjectsAsyncAdvanced<T> (
            string fileName,
            T[] objects,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        )
            where T : IPersistentObjectAsync {
            if (objects is null) {
                Debug.LogError(ObjectsArrayIsNullMessage(SAVING_OPERATION));
                source?.Dispose();
                return;
            }

            if (objects.Length is 0) {
                Debug.LogWarning(ObjectsArrayIsEmptyMessage(SAVING_OPERATION));
                source?.Dispose();
                return;
            }

            await using var unityAsyncWriter = GetUnityAsyncWriter(fileName);

            var completedTasks = 0f;
            source ??= new CancellationTokenSource();

            try {
                foreach (var obj in objects) {
                    if (source.IsCancellationRequested)
                        throw new OperationCanceledException();
                    await obj.Save(unityAsyncWriter);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Length);
                }

                onComplete?.Invoke();
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                await unityAsyncWriter.DisposeAsync();
                File.Delete(unityAsyncWriter.localPath);
                Debug.LogWarning(CancelSavingMessage);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            finally {
                source.Dispose();
            }
        }

        #endregion



        #region LoadingAsyncAdvanced

        /// <inheritdoc cref="LoadObjectAsync{T}"/>
        public static async UniTask<bool> LoadObjectAsyncAdvanced<T> (
            string fileName,
            T obj,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObjectAsync {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(LOADING_OPERATION));
                return false;
            }

            return await LoadObjectsAsyncAdvanced(fileName, new[] {obj}, null, source, onComplete);
        }


        /// <inheritdoc cref="LoadObjectsAsync{T}"/>
        public static async UniTask<bool> LoadObjectsAsyncAdvanced<T> (
            string fileName,
            T[] objects,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        )
            where T : IPersistentObjectAsync {
            if (objects is null) {
                Debug.LogError(ObjectsArrayIsNullMessage(LOADING_OPERATION));
                source?.Dispose();
                return false;
            }

            if (objects.Length is 0) {
                Debug.LogWarning(ObjectsArrayIsEmptyMessage(LOADING_OPERATION));
                source?.Dispose();
                return false;
            }

            using var unityAsyncReader = GetUnityAsyncReader(fileName);

            if (unityAsyncReader is null) {
                source?.Dispose();
                return false;
            }

            var completedTasks = 0f;
            source ??= new CancellationTokenSource();

            try {
                foreach (var obj in objects) {
                    if (source.IsCancellationRequested)
                        throw new OperationCanceledException();
                    await obj.Load(unityAsyncReader);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Length);
                }

                onComplete?.Invoke();
                return true;
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                Debug.Log(CancelLoadingMessage);
                return false;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                return false;
            }
            finally {
                source.Dispose();
            }
        }

        #endregion

    }

}