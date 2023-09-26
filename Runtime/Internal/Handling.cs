using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Internal {

    internal static class Handling {

        internal static void SaveObjects<TO> (
            ICollection<TO> objects, UnityWriter writer, IProgress<float> progress
        ) where TO : IPersistentObject {
            var completedTasks = 0f;

            foreach (TO obj in objects) {
                obj.Save(writer);
                completedTasks++;
                progress?.Report(completedTasks / objects.Count);
            }
        }


        internal static void LoadObjects<TO> (
            ICollection<TO> objects, UnityReader reader, IProgress<float> progress
        ) where TO : IPersistentObject {
            var completedTasks = 0f;

            foreach (TO obj in objects) {
                obj.Load(reader);
                completedTasks++;
                progress?.Report(completedTasks / objects.Count);
            }
        }


        internal static async UniTask<HandlingResult> SaveObjectsAsync<TO> (
            ICollection<TO> objects,
            UnityWriter writer,
            IProgress<float> progress,
            CancellationToken token
        ) where TO : IPersistentObjectAsync {
            return await Catcher.TryHandle(SaveObjectsFunc, "Saving was cancelled while data was being written");

            async UniTask SaveObjectsFunc () {
                var completedTasks = 0f;

                foreach (TO obj in objects) {
                    if (token.IsCancellationRequested) 
                        throw new OperationCanceledException();
                    await obj.Save(writer);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Count);
                }
            }
        }


        internal static async UniTask<HandlingResult> LoadStaticObjectsAsync<TO> (
            ICollection<TO> objects,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) where TO : IPersistentObjectAsync {
            return await Catcher.TryHandle(LoadObjectsFunc, "Loading was cancelled while reading data");

            async UniTask LoadObjectsFunc () {
                var completedTasks = 0f;

                foreach (TO obj in objects) {
                    if (token.IsCancellationRequested) 
                        throw new OperationCanceledException();
                    await obj.Load(reader);
                    completedTasks++;
                    progress?.Report(completedTasks / objects.Count);
                }
            }
        }


        internal static async UniTask<HandlingResult> LoadDynamicObjectsAsync<T, TO> (
            Func<TO> factoryFunc,
            AbstractHandler<T, TO> handler,
            int dynamicObjectsCount,
            UnityReader reader,
            IProgress<float> progress,
            CancellationToken token
        ) where TO : IPersistentObjectAsync where T : AbstractHandler<T, TO> {
            return await Catcher.TryHandle(LoadObjectsFunc, "Loading was cancelled while reading data");

            async UniTask LoadObjectsFunc () {
                var completedTasks = 0f;

                for (var i = 0; i < dynamicObjectsCount; i++) {
                    if (token.IsCancellationRequested) 
                        throw new OperationCanceledException();
                    TO obj = factoryFunc();
                    handler.AddObject(obj);
                    await obj.Load(reader);
                    completedTasks++;
                    progress?.Report(completedTasks / dynamicObjectsCount);
                }
            }
        }

    }

}