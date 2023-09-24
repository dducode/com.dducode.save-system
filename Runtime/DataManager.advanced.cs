using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
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

            /// <inheritdoc cref="DataManager.SaveObjectAsync"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async UniTask SaveObjectAsync (
                string fileName,
                IPersistentObjectAsync obj,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) {
                await SaveObjectsAsync(fileName, new[] {obj}, null, source, onComplete);
            }


            /// <inheritdoc cref="DataManager.SaveObjectsAsync"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async UniTask SaveObjectsAsync (
                string fileName,
                IPersistentObjectAsync[] objects,
                IProgress<float> progress = null,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) {
                await using UnityWriter unityWriter = UnityHandlersProvider.GetWriter(fileName);
                source ??= new CancellationTokenSource();

                if (source.IsCancellationRequested)
                    return;

                HandlingResult result = await InternalHandling.Advanced.TrySaveObjectsAsync(
                    objects, unityWriter, progress, source.Token
                );

                if (result == HandlingResult.Success) {
                    await unityWriter.WriteBufferToFileAsync();
                    onComplete?.Invoke();
                }
            }

            #endregion



            #region LoadingAsync

            /// <inheritdoc cref="DataManager.LoadObjectAsync"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async UniTask<bool> LoadObjectAsync (
                string fileName,
                IPersistentObjectAsync obj,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) {
                return await LoadObjectsAsync(fileName, new[] {obj}, null, source, onComplete);
            }


            /// <inheritdoc cref="DataManager.LoadObjectsAsync"/>
            [Obsolete(ObsoleteMessageAdvanced)]
            public static async UniTask<bool> LoadObjectsAsync (
                string fileName,
                IPersistentObjectAsync[] objects,
                IProgress<float> progress = null,
                CancellationTokenSource source = null,
                Action onComplete = null
            ) {
                source ??= new CancellationTokenSource();
                if (source.IsCancellationRequested)
                    return false;

                using UnityReader unityReader = UnityHandlersProvider.GetReader(fileName);

                if (await unityReader.ReadFileDataToBufferAsync()) {
                    HandlingResult result = await InternalHandling.Advanced.TryLoadStaticObjectsAsync(
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