using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SaveSystem {

    internal static class AdvancedHandler {

        internal static async UniTask<bool> TryHandleObjectsAsync<T> (
            T[] objects,
            IUnityAsyncHandler asyncHandler,
            IProgress<float> progress = null,
            CancellationTokenSource tokenSource = null
        ) where T : IPersistentObjectAsync {
            try {
                await HandleObjectsAsync(objects, asyncHandler, progress, tokenSource);
                return true;
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                if (asyncHandler is UnityAsyncWriter writer) {
                    await writer.DisposeAsync();
                    File.Delete(writer.localPath);
                }

                return false;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                return false;
            }
            finally {
                tokenSource?.Dispose();
            }
        }


        private static async UniTask HandleObjectsAsync<T> (
            IReadOnlyCollection<T> objects,
            IUnityAsyncHandler asyncHandler,
            IProgress<float> progress = null,
            CancellationTokenSource source = null
        ) where T : IPersistentObjectAsync {
            var completedTasks = 0f;
            source ??= new CancellationTokenSource();

            foreach (var obj in objects) {
                if (source.IsCancellationRequested)
                    throw new OperationCanceledException();
                await HandleObjectAsync(obj, asyncHandler);
                completedTasks++;
                progress?.Report(completedTasks / objects.Count);
            }
        }


        private static async UniTask HandleObjectAsync<T> (T obj, IUnityAsyncHandler asyncHandler)
            where T : IPersistentObjectAsync {
            switch (asyncHandler) {
                case UnityAsyncWriter asyncWriter:
                    await obj.Save(asyncWriter);
                    break;
                case UnityAsyncReader asyncReader:
                    await obj.Load(asyncReader);
                    break;
            }
        }

    }

}