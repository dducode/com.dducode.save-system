using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;


namespace SaveSystem {

    /// <summary>
    /// Main class for handling data
    /// </summary>
    public static partial class DataManager {

        private const string ObsoleteMessage =
            "It's the obsolete method and it may be removed later. Use methods of the HandlersProvider class instead this";



        #region Saving

        /// <summary>
        /// Save one object
        /// </summary>
        /// <param name="filePath"> the file where the object data will be saved </param>
        /// <param name="obj"> the object which will be saved </param>
        public static void SaveObject<T> (string filePath, T obj) where T : IPersistentObject {
            SaveObjects(filePath, new[] {obj});
        }


        /// <summary>
        /// Save many objects
        /// </summary>
        /// <param name="filePath"> the file where the object data will be saved </param>
        /// <param name="objects"> the objects which will be saved </param>
        public static void SaveObjects<T> (string filePath, IEnumerable<T> objects)
            where T : IPersistentObject {
            using UnityWriter unityWriter = UnityHandlersFactory.CreateWriter(filePath);

            foreach (T obj in objects)
                obj.Save(unityWriter);

            unityWriter.WriteBufferToFile();
        }

        #endregion



        #region Loading

        /// <summary>
        /// Load one object
        /// </summary>
        /// <param name="filePath"> the file whereof the object data will be load </param>
        /// <param name="obj"> the object which will be load </param>
        /// <returns> Returns true if there is saved data, otherwise false </returns>
        public static bool LoadObject<T> (string filePath, T obj) where T : IPersistentObject {
            return LoadObjects(filePath, new[] {obj});
        }


        /// <summary>
        /// Load many objects
        /// </summary>
        /// <param name="filePath"> the file whereof the object data will be load </param>
        /// <param name="objects"> the objects which will be load </param>
        /// <returns> Returns true if there is saved data, otherwise false </returns>
        public static bool LoadObjects<T> (string filePath, IEnumerable<T> objects)
            where T : IPersistentObject {
            using UnityReader unityReader = UnityHandlersFactory.CreateReader(filePath);

            if (unityReader.ReadFileDataToBuffer()) {
                foreach (T obj in objects)
                    obj.Load(unityReader);

                return true;
            }

            return false;
        }

        #endregion



        #region SavingAsync

        /// <summary>
        /// Save one object async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when saving is canceled </exception>
        /// <inheritdoc cref="SaveObject{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async UniTask SaveObjectAsync (
            string filePath,
            IPersistentObject obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) {
            await SaveObjectsAsync(filePath, new[] {obj}, asyncMode, null, source, onComplete);
        }


        /// <summary>
        /// Save many objects async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when saving is canceled </exception>
        /// <inheritdoc cref="SaveObjects{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async UniTask SaveObjectsAsync (
            string filePath,
            IPersistentObject[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) {
            source ??= new CancellationTokenSource();
            if (source.IsCancellationRequested)
                return;

            await using UnityWriter unityWriter = UnityHandlersFactory.CreateWriter(filePath);

            HandlingResult result;

            switch (asyncMode) {
                case AsyncMode.OnPlayerLoop:
                    var completedTasks = 0f;

                    foreach (IPersistentObject obj in objects) {
                        obj.Save(unityWriter);
                        completedTasks++;
                        progress?.Report(completedTasks / objects.Length);
                        await UniTask.NextFrame(source.Token);
                    }

                    result = source.IsCancellationRequested ? HandlingResult.CanceledOperation : HandlingResult.Success;

                    break;
                case AsyncMode.OnThreadPool:
                    await UniTask.RunOnThreadPool(() => { SaveObjects(filePath, objects); },
                        cancellationToken: source.Token);

                    result = source.IsCancellationRequested ? HandlingResult.CanceledOperation : HandlingResult.Success;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(asyncMode), asyncMode, null);
            }

            if (result == HandlingResult.Success) {
                await unityWriter.WriteBufferToFileAsync();
                onComplete?.Invoke();
            }
        }

        #endregion



        #region LoadingAsync

        /// <summary>
        /// Load one object async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when loading is canceled </exception>
        /// <inheritdoc cref="LoadObject{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async UniTask<bool> LoadObjectAsync (
            string filePath,
            IPersistentObject obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) {
            return await LoadObjectsAsync(filePath, new[] {obj}, asyncMode, null, source, onComplete);
        }


        /// <summary>
        /// Load many objects async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when loading is canceled </exception>
        /// <inheritdoc cref="LoadObjects{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async UniTask<bool> LoadObjectsAsync (
            string filePath,
            IPersistentObject[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) {
            source ??= new CancellationTokenSource();
            if (source.IsCancellationRequested)
                return false;

            using UnityReader unityReader = UnityHandlersFactory.CreateReader(filePath);

            if (await unityReader.ReadFileDataToBufferAsync()) {
                HandlingResult result;

                switch (asyncMode) {
                    case AsyncMode.OnPlayerLoop:
                        var completedTasks = 0f;

                        foreach (IPersistentObject obj in objects) {
                            obj.Load(unityReader);
                            completedTasks++;
                            progress?.Report(completedTasks / objects.Length);
                            await UniTask.NextFrame(source.Token);
                        }

                        result = source.IsCancellationRequested
                            ? HandlingResult.CanceledOperation
                            : HandlingResult.Success;

                        break;
                    case AsyncMode.OnThreadPool:
                        await UniTask.RunOnThreadPool(() => { LoadObjects(filePath, objects); },
                            cancellationToken: source.Token);

                        result = source.IsCancellationRequested
                            ? HandlingResult.CanceledOperation
                            : HandlingResult.Success;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(asyncMode), asyncMode, null);
                }

                if (result == HandlingResult.Success) {
                    onComplete?.Invoke();
                    return true;
                }
            }

            return false;
        }

        #endregion

    }

}