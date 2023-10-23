using System;
using System.Threading;
using JetBrains.Annotations;
using SaveSystem.Handlers;
using SaveSystem.Internal;
using SaveSystem.UnityHandlers;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using TaskAlias = Cysharp.Threading.Tasks.UniTask;
using TaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
using TaskAlias = System.Threading.Tasks.Task;
using TaskBool = System.Threading.Tasks.Task<bool>;
#endif


namespace SaveSystem {

    public static partial class DataManager {

        /// <summary>
        /// Contains the methods for advanced objects handling
        /// <para>
        /// <a href="https://dducode.github.io/save-system-docs/manual/advanced.html">See manual</a>
        /// </para>
        /// </summary>
        public static class Advanced {

            private const string ObsoleteMessageAdvanced =
                "The method is obsolete and may be removed later. Use the AdvancedDataHandler instead this";



            #region SavingAsync

            /// <inheritdoc cref="DataManager.SaveObjectAsync"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async TaskAlias SaveObjectAsync (
                [NotNull] string filePath,
                [NotNull] IPersistentObjectAsync obj,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) {
                if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
                if (obj == null) throw new ArgumentNullException(nameof(obj));
                await SaveObjectsAsync(filePath, new[] {obj}, null, source, onComplete);
            }


            /// <inheritdoc cref="DataManager.SaveObjectsAsync"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async TaskAlias SaveObjectsAsync (
                [NotNull] string filePath,
                [NotNull] IPersistentObjectAsync[] objects,
                IProgress<float> progress = null,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) {
                if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
                if (objects == null) throw new ArgumentNullException(nameof(objects));
                if (objects.Length == 0)
                    throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));

                await using UnityWriter unityWriter = UnityHandlersFactory.CreateBufferingWriter(filePath);
                source ??= new CancellationTokenSource();

                if (source.IsCancellationRequested)
                    return;

                HandlingResult result;

                try {
                    var completedTasks = 0f;

                    foreach (IPersistentObjectAsync obj in objects) {
                        source.Token.ThrowIfCancellationRequested();
                        await obj.Save(unityWriter);
                        completedTasks++;
                        progress?.Report(completedTasks / objects.Length);
                    }

                    result = HandlingResult.Success;
                }
                catch (OperationCanceledException) {
                    Logger.LogWarning("Saving was cancelled while data was being written");
                    result = HandlingResult.CanceledOperation;
                }

                if (result == HandlingResult.Success) {
                    await unityWriter.WriteBufferToFileAsync();
                    onComplete?.Invoke();
                }
            }

            #endregion



            #region LoadingAsync

            /// <inheritdoc cref="DataManager.LoadObjectAsync"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async TaskBool LoadObjectAsync (
                [NotNull] string filePath,
                [NotNull] IPersistentObjectAsync obj,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) {
                if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
                if (obj == null) throw new ArgumentNullException(nameof(obj));
                return await LoadObjectsAsync(filePath, new[] {obj}, null, source, onComplete);
            }


            /// <inheritdoc cref="DataManager.LoadObjectsAsync"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async TaskBool LoadObjectsAsync (
                [NotNull] string filePath,
                [NotNull] IPersistentObjectAsync[] objects,
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
                    HandlingResult result = await Handling.LoadStaticObjectsAsync(
                        objects, unityReader, progress, source.Token
                    );

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

}