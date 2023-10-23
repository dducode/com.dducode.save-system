using System;
using System.Collections.Generic;
using System.Threading;
using SaveSystem.Handlers;
using SaveSystem.UnityHandlers;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskResult = Cysharp.Threading.Tasks.UniTask<SaveSystem.Handlers.HandlingResult>;
#else
using TaskResult = System.Threading.Tasks.Task<SaveSystem.Handlers.HandlingResult>;
#endif

namespace SaveSystem.Internal {

    internal static class Handling {

        internal static void SaveObjects<THandler, TObject> (
            AbstractHandler<THandler, TObject> handler, UnityWriter writer, IProgress<float> progress
        ) where TObject : IPersistentObject where THandler : AbstractHandler<THandler, TObject> {
            var completedTasks = 0f;

            foreach (TObject obj in handler) {
                obj.Save(writer);
                progress?.Report(++completedTasks / handler.ObjectsCount);
            }
        }


        internal static void LoadObjects<THandler, TObject> (
            AbstractHandler<THandler, TObject> handler, UnityReader reader, IProgress<float> progress
        ) where TObject : IPersistentObject where THandler : AbstractHandler<THandler, TObject> {
            var completedTasks = 0f;

            foreach (TObject obj in handler) {
                obj.Load(reader);
                progress?.Report(++completedTasks / handler.ObjectsCount);
            }
        }


        internal static async TaskResult SaveObjectsAsync<TObject, THandler> (
            AbstractHandler<THandler, TObject> handler,
            UnityWriter writer,
            IProgress<float> progress,
            CancellationToken token
        ) where TObject : IPersistentObjectAsync where THandler : AbstractHandler<THandler, TObject> {
            try {
                var completedTasks = 0f;

                foreach (TObject obj in handler) {
                    token.ThrowIfCancellationRequested();
                    await obj.Save(writer);
                    progress?.Report(++completedTasks / handler.ObjectsCount);
                }

                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Saving was cancelled while data was being written");
                return HandlingResult.CanceledOperation;
            }
        }


        internal static async TaskResult LoadStaticObjectsAsync<TObject> (
            ICollection<TObject> objects,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) where TObject : IPersistentObjectAsync {
            try {
                var completedTasks = 0f;

                foreach (TObject obj in objects) {
                    token.ThrowIfCancellationRequested();
                    await obj.Load(reader);
                    progress?.Report(++completedTasks / objects.Count);
                }

                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Loading was cancelled while reading data");
                return HandlingResult.CanceledOperation;
            }
        }


        internal static async TaskResult LoadDynamicObjectsAsync<THandler, TObject> (
            Func<TObject> factoryFunc,
            AbstractHandler<THandler, TObject> handler,
            int dynamicObjectsCount,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) where TObject : IPersistentObjectAsync where THandler : AbstractHandler<THandler, TObject> {
            try {
                var completedTasks = 0f;
                var cachedObjects = new TObject[dynamicObjectsCount];

                for (var i = 0; i < dynamicObjectsCount; i++) {
                    token.ThrowIfCancellationRequested();
                    TObject obj = factoryFunc();
                    cachedObjects[i] = obj;
                    await obj.Load(reader);
                    progress?.Report(++completedTasks / dynamicObjectsCount);
                }

                handler.AddObjects(cachedObjects);
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Loading was cancelled while reading data");
                return HandlingResult.CanceledOperation;
            }
        }

    }

}