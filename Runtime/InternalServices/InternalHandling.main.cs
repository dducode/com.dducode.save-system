using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.InternalServices {

    internal static partial class InternalHandling {

        internal static async UniTask<bool> TryHandleObjectsAsync<T> (
            T[] objects,
            AsyncMode asyncMode,
            IUnityHandler handler,
            IProgress<float> progress = null,
            CancellationTokenSource tokenSource = null
        ) where T : IPersistentObject {
            try {
                await HandleObjectsAsync(objects, asyncMode, handler, progress, tokenSource);
                return true;
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                switch (handler) {
                    case UnityWriter writer:
                        await writer.DisposeAsync();
                        File.Delete(writer.localPath);
                        InternalLogger.LogWarning("Save cancelled. The data file has been deleted");
                        break;
                    case UnityReader:
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
            AsyncMode asyncMode,
            IUnityHandler handler,
            IProgress<float> progress = null,
            CancellationTokenSource tokenSource = null
        ) where T : IPersistentObject {
            var completedTasks = 0f;
            tokenSource ??= new CancellationTokenSource();

            switch (asyncMode) {
                case AsyncMode.OnPlayerLoop:
                    foreach (T obj in objects) {
                        HandleObject(obj, handler);
                        completedTasks++;
                        progress?.Report(completedTasks / objects.Count);
                        await UniTask.NextFrame(tokenSource.Token);
                    }

                    break;
                case AsyncMode.OnThreadPool:
                    await UniTask.RunOnThreadPool(() => {
                        foreach (T obj in objects) {
                            if (tokenSource.IsCancellationRequested)
                                throw new OperationCanceledException();
                            HandleObject(obj, handler);
                            completedTasks++;
                            progress?.Report(completedTasks / objects.Count);
                        }
                    });

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private static void HandleObject<T> (T obj, IUnityHandler handler) where T : IPersistentObject {
            switch (handler) {
                case UnityWriter writer:
                    obj.Save(writer);
                    break;
                case UnityReader reader:
                    obj.Load(reader);
                    break;
            }
        }

    }

}