using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Object Handler can help you to saving/loading <see cref="IPersistentObject">persistent objects</see>.
    /// Also you can set parameters for this as a chain of methods
    /// </summary>
    public sealed class ObjectHandler<TO> : AbstractHandler<ObjectHandler<TO>, TO>, IObjectHandler,
        IAsyncObjectHandler where TO : IPersistentObject {

        internal ObjectHandler (string localFilePath, TO[] staticObjects) : base(localFilePath, staticObjects) { }


        internal ObjectHandler (string localFilePath, Func<TO> factoryFunc) : base(localFilePath, factoryFunc) { }


        public void Save () {
            using UnityWriter unityWriter = UnityHandlersProvider.GetWriter(destinationPath);

            var savedObjects = new List<TO>(dynamicObjects);
            unityWriter.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            var completedTasks = 0f;

            foreach (TO obj in savedObjects) {
                obj.Save(unityWriter);
                completedTasks++;
                savingProgress?.Report(completedTasks / savedObjects.Count);
            }

            unityWriter.WriteBufferToFile();
        }


        public HandlingResult Load () {
            using UnityReader unityReader = UnityHandlersProvider.GetReader(destinationPath);

            if (unityReader.ReadFileDataToBuffer()) {
                int dynamicObjectsCount = unityReader.ReadInt();
                List<TO> loadedObjects = SpawnObjects(dynamicObjectsCount);
                AddObjects(loadedObjects);
                loadedObjects.AddRange(staticObjects);

                var completedTasks = 0f;

                foreach (TO obj in loadedObjects) {
                    obj.Load(unityReader);
                    completedTasks++;
                    loadingProgress?.Report(completedTasks / loadedObjects.Count);
                }

                return HandlingResult.Success;
            }

            return HandlingResult.FileNotExists;
        }


        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj == null);
            var savedObjects = new List<TO>(dynamicObjects);

            await using UnityWriter unityWriter = UnityHandlersProvider.GetWriter(destinationPath);
            unityWriter.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            HandlingResult result = await InternalHandling.TrySaveObjectsAsync(
                savedObjects, unityWriter, savingProgress, token
            );

            if (result == HandlingResult.Success)
                await unityWriter.WriteBufferToFileAsync();

            return result;
        }


        public async UniTask<HandlingResult> LoadAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader unityReader = UnityHandlersProvider.GetReader(destinationPath);

            if (await unityReader.ReadFileDataToBufferAsync()) {
                int dynamicObjectsCount = unityReader.ReadInt();
                if (dynamicObjectsCount > 0 && factoryFunc == null)
                    throw new ArgumentException(nameof(factoryFunc));

                HandlingResult result;

                if (dynamicObjectsCount > 0) {
                    result = await InternalHandling.TryLoadDynamicObjectsAsync(
                        factoryFunc, this, dynamicObjectsCount, unityReader, loadingProgress, token
                    );

                    if (result != HandlingResult.Success)
                        return result;
                }

                result = await InternalHandling.TryLoadStaticObjectsAsync(
                    staticObjects, unityReader, loadingProgress, token
                );

                return result;
            }

            return HandlingResult.FileNotExists;
        }


        private List<TO> SpawnObjects (int count) {
            if (count > 0 && factoryFunc == null)
                throw new ArgumentException(nameof(factoryFunc));

            var objects = new List<TO>();

            for (var i = 0; i < count; i++) {
                TO obj = factoryFunc();
                objects.Add(obj);
            }

            return objects;
        }

    }

}