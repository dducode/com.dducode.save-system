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

            internal static async UniTask<HandlingResult> TrySaveObjectsAsync (
                IReadOnlyCollection<IPersistentObjectAsync> objects,
                UnityWriter writer,
                IProgress<float> progress,
                CancellationToken token
            ) {
                try {
                    await SaveObjectsAsync(objects, writer, progress, token);
                    await writer.WriteBufferToFileAsync();
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


            internal static async UniTask<HandlingResult> TryLoadObjectsAsync (
                IReadOnlyCollection<IPersistentObjectAsync> objects,
                UnityReader reader,
                IProgress<float> progress,
                CancellationToken token
            ) {
                try {
                    await LoadObjectsAsync(objects, reader, progress, token);
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


            private static async UniTask SaveObjectsAsync (
                IReadOnlyCollection<IPersistentObjectAsync> objects,
                UnityWriter writer,
                IProgress<float> progress,
                CancellationToken token
            ) {
                var completedTasks = 0f;

                foreach (IPersistentObjectAsync obj in objects) {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException();
                    await obj.Save(writer);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Count);
                }
            }


            private static async UniTask LoadObjectsAsync (
                IReadOnlyCollection<IPersistentObjectAsync> objects,
                UnityReader reader,
                IProgress<float> progress,
                CancellationToken token
            ) {
                var completedTasks = 0f;

                foreach (IPersistentObjectAsync obj in objects) {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException();
                    await obj.Load(reader);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Count);
                }
            }

        }

    }

}