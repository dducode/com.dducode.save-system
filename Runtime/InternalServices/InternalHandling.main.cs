using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.InternalServices {

    internal static partial class InternalHandling {

        public static async UniTask<HandlingResult> TrySaveObjectsAsync (
            IPersistentObject[] objects,
            AsyncMode asyncMode,
            UnityWriter writer,
            IProgress<float> progress,
            CancellationToken tokenSource
        ) {
            try {
                switch (asyncMode) {
                    case AsyncMode.OnPlayerLoop:
                        await SaveObjectsOnPlayerLoop(objects, writer, progress, tokenSource);
                        break;
                    case AsyncMode.OnThreadPool:
                        await SaveObjectsOnThreadPool(objects, writer, progress, tokenSource);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return HandlingResult.Success;
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                InternalLogger.LogWarning("Saving was cancelled while data was being written");
                return HandlingResult.CanceledOperation;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return HandlingResult.UnknownError;
            }
        }


        public static async UniTask<HandlingResult> TryLoadObjectsAsync (
            IPersistentObject[] objects,
            AsyncMode asyncMode,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) {
            try {
                switch (asyncMode) {
                    case AsyncMode.OnPlayerLoop:
                        await LoadObjectsOnPlayerLoop(objects, reader, progress, token);
                        break;
                    case AsyncMode.OnThreadPool:
                        await LoadObjectsOnThreadPool(objects, reader, progress, token);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return HandlingResult.Success;
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                InternalLogger.LogWarning("Loading was cancelled while reading data");
                return HandlingResult.CanceledOperation;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return HandlingResult.UnknownError;
            }
        }


        private static async UniTask SaveObjectsOnPlayerLoop (
            IReadOnlyCollection<IPersistentObject> objects,
            UnityWriter writer,
            IProgress<float> progress,
            CancellationToken token
        ) {
            var completedTasks = 0f;

            foreach (IPersistentObject obj in objects) {
                obj.Save(writer);
                completedTasks++;
                progress?.Report(completedTasks / objects.Count);
                await UniTask.NextFrame(token);
            }
        }


        private static async UniTask SaveObjectsOnThreadPool (
            IReadOnlyCollection<IPersistentObject> objects,
            UnityWriter writer,
            IProgress<float> progress,
            CancellationToken token
        ) {
            var completedTasks = 0f;

            await UniTask.RunOnThreadPool(() => {
                foreach (IPersistentObject obj in objects) {
                    obj.Save(writer);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Count);
                }
            }, cancellationToken: token);
        }


        private static async UniTask LoadObjectsOnPlayerLoop (
            IReadOnlyCollection<IPersistentObject> objects,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) {
            var completedTasks = 0f;

            foreach (IPersistentObject obj in objects) {
                obj.Load(reader);
                completedTasks++;
                progress?.Report(completedTasks / objects.Count);
                await UniTask.NextFrame(token);
            }
        }


        private static async UniTask LoadObjectsOnThreadPool (
            IReadOnlyCollection<IPersistentObject> objects,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) {
            var completedTasks = 0f;

            await UniTask.RunOnThreadPool(() => {
                foreach (IPersistentObject obj in objects) {
                    obj.Load(reader);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Count);
                }
            }, cancellationToken: token);
        }

    }

}