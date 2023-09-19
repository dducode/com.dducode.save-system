using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

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

            /// <inheritdoc cref="DataManager.SaveObjectAsync{T}"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async UniTask SaveObjectAsync<T> (
                string fileName,
                T obj,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) where T : IPersistentObjectAsync {
                await SaveObjectsAsync(fileName, new[] {obj}, null, source, onComplete);
            }


            /// <inheritdoc cref="DataManager.SaveObjectsAsync{T}"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async UniTask SaveObjectsAsync<T> (
                string fileName,
                T[] objects,
                IProgress<float> progress = null,
                CancellationTokenSource source = null,
                Action onComplete = null
            )
                where T : IPersistentObjectAsync {
                await using UnityAsyncWriter unityAsyncWriter = UnityAsyncWriter.GetLocal(fileName);

                if (await InternalHandling.Advanced.TryHandleObjectsAsync(objects, unityAsyncWriter, progress, source))
                    onComplete?.Invoke();
            }

            #endregion



            #region LoadingAsync

            /// <inheritdoc cref="DataManager.LoadObjectAsync{T}"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async UniTask<bool> LoadObjectAsync<T> (
                string fileName,
                T obj,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) where T : IPersistentObjectAsync {
                return await LoadObjectsAsync(fileName, new[] {obj}, null, source, onComplete);
            }


            /// <inheritdoc cref="DataManager.LoadObjectsAsync{T}"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async UniTask<bool> LoadObjectsAsync<T> (
                string fileName,
                T[] objects,
                IProgress<float> progress = null,
                CancellationTokenSource source = null,
                Action onComplete = null
            )
                where T : IPersistentObjectAsync {
                using UnityAsyncReader unityAsyncReader = UnityAsyncReader.GetLocal(fileName);

                if (unityAsyncReader is null) {
                    source?.Dispose();
                    return false;
                }

                if (await InternalHandling.Advanced.TryHandleObjectsAsync(objects, unityAsyncReader, progress,
                    source)) {
                    onComplete?.Invoke();
                    return true;
                }

                return false;
            }

            #endregion

        }

    }

}