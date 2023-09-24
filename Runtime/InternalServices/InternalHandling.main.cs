using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.InternalServices {

    internal static partial class InternalHandling {

        public static async UniTask<HandlingResult> TrySaveObjectsAsync<TO> (
            ICollection<TO> objects,
            UnityWriter writer,
            IProgress<float> progress,
            CancellationToken tokenSource
        ) where TO : IPersistentObject {
            try {
                await SaveObjects(objects, writer, progress, tokenSource);
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


        public static async UniTask<HandlingResult> TryLoadStaticObjectsAsync<TO> (
            ICollection<TO> objects,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) where TO : IPersistentObject {
            try {
                await LoadStaticObjects(objects, reader, progress, token);
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


        public static async UniTask<HandlingResult> TryLoadDynamicObjectsAsync<T, TO> (
            Func<TO> factoryFunc,
            AbstractHandler<T, TO> handler,
            int objectsCount,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) where TO : IPersistentObject where T : AbstractHandler<T, TO> {
            try {
                await LoadDynamicObjects(
                    factoryFunc, handler, objectsCount, reader, progress, token
                );
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


        private static async UniTask SaveObjects<TO> (
            ICollection<TO> objects,
            UnityWriter writer,
            IProgress<float> progress,
            CancellationToken token
        ) where TO : IPersistentObject {
            var completedTasks = 0f;

            foreach (TO obj in objects) {
                obj.Save(writer);
                completedTasks++;
                progress?.Report(completedTasks / objects.Count);
                await UniTask.NextFrame(token);
            }
        }


        private static async UniTask LoadStaticObjects<TO> (
            ICollection<TO> objects,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) where TO : IPersistentObject {
            var completedTasks = 0f;

            foreach (TO obj in objects) {
                obj.Load(reader);
                completedTasks++;
                progress?.Report(completedTasks / objects.Count);
                await UniTask.NextFrame(token);
            }
        }


        private static async UniTask LoadDynamicObjects<T, TO> (
            Func<TO> factoryFunc,
            AbstractHandler<T, TO> handler,
            int objectsCount,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) where TO : IPersistentObject where T : AbstractHandler<T, TO> {
            var completedTasks = 0f;

            for (var i = 0; i < objectsCount; i++) {
                TO obj = factoryFunc();
                handler.AddObject(obj);
                obj.Load(reader);
                completedTasks++;
                progress?.Report(completedTasks / objectsCount);
                await UniTask.NextFrame(token);
            }
        }

    }

}