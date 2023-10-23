using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using SaveSystem.Handlers;
using SaveSystem.Internal;
using SaveSystem.UnityHandlers;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using TaskAlias = Cysharp.Threading.Tasks.UniTask;
using TaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
using System.Threading.Tasks;
using TaskAlias = System.Threading.Tasks.Task;
using TaskBool = System.Threading.Tasks.Task<bool>;
#endif


namespace SaveSystem {

    /// <summary>
    /// Main class for handling data
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class DataManager {

        private const string ObsoleteMessage =
            "It's the obsolete method and it may be removed later. Use methods of the HandlersProvider class instead this.";



        #region Saving

        /// <summary>
        /// Save one object
        /// </summary>
        /// <param name="filePath"> the file where the object data will be saved </param>
        /// <param name="obj"> the object which will be saved </param>
        public static void SaveObject<T> ([NotNull] string filePath, [NotNull] T obj) where T : IPersistentObject {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            SaveObjects(filePath, new[] {obj});
        }


        /// <summary>
        /// Save many objects
        /// </summary>
        /// <param name="filePath"> the file where the object data will be saved </param>
        /// <param name="objects"> the objects which will be saved </param>
        public static void SaveObjects<T> ([NotNull] string filePath, [NotNull] ICollection<T> objects)
            where T : IPersistentObject {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            if (objects.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));
            using UnityWriter unityWriter = UnityHandlersFactory.CreateDirectWriter(filePath);

            foreach (T obj in objects)
                obj.Save(unityWriter);
        }

        #endregion



        #region Loading

        /// <summary>
        /// Load one object
        /// </summary>
        /// <param name="filePath"> the file whereof the object data will be load </param>
        /// <param name="obj"> the object which will be load </param>
        /// <returns> Returns true if there is saved data, otherwise false </returns>
        public static bool LoadObject<T> ([NotNull] string filePath, [NotNull] T obj) where T : IPersistentObject {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return LoadObjects(filePath, new[] {obj});
        }


        /// <summary>
        /// Load many objects
        /// </summary>
        /// <param name="filePath"> the file whereof the object data will be load </param>
        /// <param name="objects"> the objects which will be load </param>
        /// <returns> Returns true if there is saved data, otherwise false </returns>
        public static bool LoadObjects<T> ([NotNull] string filePath, [NotNull] ICollection<T> objects)
            where T : IPersistentObject {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            if (objects.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));
            using UnityReader unityReader = UnityHandlersFactory.CreateDirectReader(filePath);

            if (unityReader != null) {
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
        public static async TaskAlias SaveObjectAsync (
            [NotNull] string filePath,
            [NotNull] IPersistentObject obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            await SaveObjectsAsync(filePath, new[] {obj}, asyncMode, null, source, onComplete);
        }


        /// <summary>
        /// Save many objects async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when saving is canceled </exception>
        /// <inheritdoc cref="SaveObjects{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async TaskAlias SaveObjectsAsync (
            [NotNull] string filePath,
            [NotNull] IPersistentObject[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            if (objects.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));
            source ??= new CancellationTokenSource();
            if (source.IsCancellationRequested)
                return;

            await using UnityWriter unityWriter = UnityHandlersFactory.CreateBufferingWriter(filePath);

            HandlingResult result;

            switch (asyncMode) {
                case AsyncMode.OnPlayerLoop:
                    var completedTasks = 0f;

                    foreach (IPersistentObject obj in objects) {
                        obj.Save(unityWriter);
                        completedTasks++;
                        progress?.Report(completedTasks / objects.Length);
                    #if SAVE_SYSTEM_UNITASK_SUPPORT
                        await UniTask.NextFrame(source.Token);
                    #else
                        await Task.Delay(1, source.Token);
                    #endif
                    }

                    result = source.IsCancellationRequested ? HandlingResult.CanceledOperation : HandlingResult.Success;

                    break;
                case AsyncMode.OnThreadPool:
                #if SAVE_SYSTEM_UNITASK_SUPPORT
                    await UniTask.RunOnThreadPool(() => SaveObjects(filePath, objects),
                        cancellationToken: source.Token);
                #else
                    await Task.Run(() => SaveObjects(filePath, objects),
                        cancellationToken: source.Token);
                #endif

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
        public static async TaskBool LoadObjectAsync (
            [NotNull] string filePath,
            [NotNull] IPersistentObject obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return await LoadObjectsAsync(filePath, new[] {obj}, asyncMode, null, source, onComplete);
        }


        /// <summary>
        /// Load many objects async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when loading is canceled </exception>
        /// <inheritdoc cref="LoadObjects{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async TaskBool LoadObjectsAsync (
            [NotNull] string filePath,
            [NotNull] IPersistentObject[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            if (objects.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));
            source ??= new CancellationTokenSource();
            if (source.IsCancellationRequested)
                return false;

            using UnityReader unityReader = UnityHandlersFactory.CreateBufferingReader(filePath);

            if (await unityReader.ReadFileDataToBufferAsync()) {
                HandlingResult result;

                switch (asyncMode) {
                    case AsyncMode.OnPlayerLoop:
                        var completedTasks = 0f;

                        foreach (IPersistentObject obj in objects) {
                            obj.Load(unityReader);
                            completedTasks++;
                            progress?.Report(completedTasks / objects.Length);
                        #if SAVE_SYSTEM_UNITASK_SUPPORT
                            await UniTask.NextFrame(source.Token);
                        #else
                            await Task.Delay(1, source.Token);
                        #endif
                        }

                        result = source.IsCancellationRequested
                            ? HandlingResult.CanceledOperation
                            : HandlingResult.Success;

                        break;
                    case AsyncMode.OnThreadPool:
                    #if SAVE_SYSTEM_UNITASK_SUPPORT
                        await UniTask.RunOnThreadPool(() => LoadObjects(filePath, objects),
                            cancellationToken: source.Token);
                    #else
                        await Task.Run(() => LoadObjects(filePath, objects),
                            cancellationToken: source.Token);
                    #endif

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