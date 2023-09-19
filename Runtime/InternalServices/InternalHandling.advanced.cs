using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.InternalServices {

    internal static partial class InternalHandling {

        internal static class Advanced {

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
                    switch (asyncHandler) {
                        case UnityAsyncWriter writer:
                            await writer.DisposeAsync();
                            File.Delete(writer.localPath);
                            InternalLogger.LogWarning("Save cancelled. The data file has been deleted");
                            break;
                        case UnityAsyncReader:
                            InternalLogger.Log("Load cancelled");
                            break;
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

                foreach (T obj in objects) {
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

}