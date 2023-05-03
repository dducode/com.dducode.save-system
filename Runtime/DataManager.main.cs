using System;
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
                Debug.LogError(ObjectIsNullMessage(nameof(SaveObject)));
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
            if (!ArrayIsValid(objects as object[], nameof(SaveObjects)))
                return;

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
                Debug.LogError(ObjectIsNullMessage(nameof(LoadObject)));
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
            if (!ArrayIsValid(objects as object[], nameof(LoadObjects)))
                return false;

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
                Debug.LogError(ObjectIsNullMessage(nameof(SaveObjectAsync)));
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
            if (!ArrayIsValid(objects as object[], nameof(SaveObjectsAsync))) {
                source?.Dispose();
                return;
            }

            await using var unityWriter = GetUnityWriter(fileName);

            if (await Handler.TryHandleObjectsAsync(objects, asyncMode, unityWriter, progress, source))
                onComplete?.Invoke();
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
                Debug.LogError(ObjectIsNullMessage(nameof(LoadObjectAsync)));
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
            if (!ArrayIsValid(objects as object[], nameof(LoadObjectsAsync))) {
                source?.Dispose();
                return false;
            }

            using var unityReader = GetUnityReader(fileName);

            if (unityReader is null) {
                source?.Dispose();
                return false;
            }

            if (await Handler.TryHandleObjectsAsync(objects, asyncMode, unityReader, progress, source)) {
                onComplete?.Invoke();
                return true;
            }

            return false;
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
                Debug.LogError(ObjectIsNullMessage(nameof(SaveObjectAsyncAdvanced)));
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
            if (!ArrayIsValid(objects as object[], nameof(SaveObjectsAsyncAdvanced))) {
                source?.Dispose();
                return;
            }

            await using var unityAsyncWriter = GetUnityAsyncWriter(fileName);

            if (await AdvancedHandler.TryHandleObjectsAsync(objects, unityAsyncWriter, progress, source))
                onComplete?.Invoke();
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
                Debug.LogError(ObjectIsNullMessage(nameof(LoadObjectAsyncAdvanced)));
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
            if (!ArrayIsValid(objects as object[], nameof(LoadObjectsAsyncAdvanced))) {
                source?.Dispose();
                return false;
            }

            using var unityAsyncReader = GetUnityAsyncReader(fileName);

            if (unityAsyncReader is null) {
                source?.Dispose();
                return false;
            }

            if (await AdvancedHandler.TryHandleObjectsAsync(objects, unityAsyncReader, progress, source)) {
                onComplete?.Invoke();
                return true;
            }

            return false;
        }

        #endregion



        #region SavingRemote

        public static async UniTask SaveObjectRemote<T> (
            string url,
            T obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(nameof(SaveObjectRemote)));
                return;
            }

            await SaveObjectsRemote(url, new[] {obj}, asyncMode, null, source, onComplete);
        }


        public static async UniTask SaveObjectsRemote<T> (
            string url,
            T[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            if (!ArrayIsValid(objects as object[], nameof(SaveObjectsRemote))) {
                source?.Dispose();
                return;
            }

            await using var unityWriter = GetUnityWriterRemote();

            if (await Handler.TryHandleObjectsAsync(objects, asyncMode, unityWriter, progress, source)) {
                await unityWriter.DisposeAsync();
                await SendDataToRemote(url);
                onComplete?.Invoke();
            }
        }

        #endregion



        #region LoadingRemote

        public static async UniTask<bool> LoadObjectRemote<T> (
            string url,
            T obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(nameof(LoadObjectRemote)));
                return false;
            }

            return await LoadObjectsRemote(url, new[] {obj}, asyncMode, null, source, onComplete);
        }


        public static async UniTask<bool> LoadObjectsRemote<T> (
            string url,
            T[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            if (!ArrayIsValid(objects as object[], nameof(LoadObjectsRemote))) {
                source?.Dispose();
                return false;
            }

            using var unityReader = await GetUnityReaderRemote(url);

            if (unityReader is null) {
                source?.Dispose();
                return false;
            }

            if (await Handler.TryHandleObjectsAsync(objects, asyncMode, unityReader, progress, source)) {
                onComplete?.Invoke();
                return true;
            }

            return false;
        }

        #endregion



        #region SavingRemoteAdvanced

        public static async UniTask SaveObjectRemoteAdvanced<T> (
            string url,
            T obj,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObjectAsync {
            if (obj is null) {
                Debug.LogError(ObjectIsNullMessage(nameof(SaveObjectRemoteAdvanced)));
                return;
            }

            await SaveObjectsRemoteAdvanced(url, new[] {obj}, null, source, onComplete);
        }


        public static async UniTask SaveObjectsRemoteAdvanced<T> (
            string url,
            T[] objects,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObjectAsync {
            if (!ArrayIsValid(objects as object[], nameof(SaveObjectsRemoteAdvanced))) {
                source?.Dispose();
                return;
            }

            await using var unityAsyncWriter = GetUnityAsyncWriterRemote();

            if (await AdvancedHandler.TryHandleObjectsAsync(objects, unityAsyncWriter, progress, source)) {
                await unityAsyncWriter.DisposeAsync();
                await SendDataToRemote(url);
                onComplete?.Invoke();
            }
        }

        #endregion



        #region LoadingRemoteAdvanced

        public static async UniTask<bool> LoadObjectRemoteAdvanced<T> (
            string url,
            T obj,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObjectAsync {
            if (obj is null) {
                source?.Dispose();
                return false;
            }

            return await LoadObjectsRemoteAdvanced(url, new[] {obj}, null, source, onComplete);
        }


        public static async UniTask<bool> LoadObjectsRemoteAdvanced<T> (
            string url,
            T[] objects,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObjectAsync {
            if (!ArrayIsValid(objects as object[], nameof(LoadObjectsRemoteAdvanced))) {
                source?.Dispose();
                return false;
            }

            using var unityAsyncReader = await GetUnityAsyncReaderRemote(url);

            if (unityAsyncReader is null) {
                source?.Dispose();
                return false;
            }

            if (await AdvancedHandler.TryHandleObjectsAsync(objects, unityAsyncReader, progress, source)) {
                onComplete?.Invoke();
                return true;
            }

            return false;
        }

        #endregion



        private static bool ArrayIsValid (object[] array, string methodName) {
            if (array is null) {
                Debug.LogError(ObjectsArrayIsNullMessage(methodName));
                return false;
            }
            else if (array.Length is 0) {
                Debug.LogWarning(ObjectsArrayIsEmptyMessage(methodName));
                return false;
            }

            return true;
        }

    }

}