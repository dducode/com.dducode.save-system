using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.InternalServices {

    internal static partial class InternalHandling {

        internal static class Advanced {

            internal static async UniTask<HandlingResult> TrySaveObjectsAsync<TO> (
                ICollection<TO> objects,
                UnityWriter writer,
                IProgress<float> progress,
                CancellationToken token
            ) where TO : IPersistentObjectAsync {
                try {
                    await SaveObjectsAsync(objects, writer, progress, token);
                    return HandlingResult.Success;
                }
                catch (Exception ex) when (ex is OperationCanceledException) {
                    InternalLogger.LogWarning("Saving was cancelled while data was being written");
                    return HandlingResult.CanceledOperation;
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                    return HandlingResult.UnknownError;
                }
            }


            internal static async UniTask<HandlingResult> TryLoadStaticObjectsAsync<TO> (
                ICollection<TO> objects,
                UnityReader reader,
                IProgress<float> progress,
                CancellationToken token
            ) where TO : IPersistentObjectAsync {
                try {
                    await LoadStaticObjectsAsync(objects, reader, progress, token);
                    return HandlingResult.Success;
                }
                catch (Exception ex) when (ex is OperationCanceledException) {
                    InternalLogger.LogWarning("Loading was cancelled while reading data");
                    return HandlingResult.CanceledOperation;
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                    return HandlingResult.UnknownError;
                }
            }


            internal static async UniTask<HandlingResult> TryLoadDynamicObjectsAsync<T, TO> (
                Func<TO> objects,
                AbstractHandler<T, TO> handler,
                int dynamicObjectsCount,
                UnityReader reader,
                IProgress<float> progress,
                CancellationToken token
            ) where TO : IPersistentObjectAsync where T : AbstractHandler<T, TO> {
                try {
                    await LoadDynamicObjectsAsync(objects, handler, dynamicObjectsCount, reader, progress, token);
                    return HandlingResult.Success;
                }
                catch (Exception ex) when (ex is OperationCanceledException) {
                    InternalLogger.LogWarning("Loading was cancelled while reading data");
                    return HandlingResult.CanceledOperation;
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                    return HandlingResult.UnknownError;
                }
            }


            private static async UniTask SaveObjectsAsync<TO> (
                ICollection<TO> objects,
                UnityWriter writer,
                IProgress<float> progress,
                CancellationToken token
            ) where TO : IPersistentObjectAsync {
                var completedTasks = 0f;

                foreach (TO obj in objects) {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException();
                    await obj.Save(writer);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Count);
                }
            }


            private static async UniTask LoadStaticObjectsAsync<TO> (
                ICollection<TO> objects,
                UnityReader reader,
                IProgress<float> progress,
                CancellationToken token
            ) where TO : IPersistentObjectAsync {
                var completedTasks = 0f;

                foreach (TO obj in objects) {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException();
                    await obj.Load(reader);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Count);
                }
            }


            private static async UniTask LoadDynamicObjectsAsync<T, TO> (
                Func<TO> factoryFunc,
                AbstractHandler<T, TO> handler,
                int dynamicObjectCount,
                UnityReader reader,
                IProgress<float> progress,
                CancellationToken token
            ) where TO : IPersistentObjectAsync where T : AbstractHandler<T, TO> {
                var completedTasks = 0f;

                for (var i = 0; i < dynamicObjectCount; i++) {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException();
                    TO obj = factoryFunc();
                    handler.AddObject(obj);
                    await obj.Load(reader);
                    completedTasks++;
                    progress?.Report(completedTasks / dynamicObjectCount);
                }
            }

        }

    }

}