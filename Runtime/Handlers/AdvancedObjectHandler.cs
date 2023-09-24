using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// It's same as <see cref="ObjectHandler{TO}">Object Handler</see> only for the IPersistentObjectAsync
    /// </summary>
    public sealed class AdvancedObjectHandler<TO> : AbstractHandler<AdvancedObjectHandler<TO>, TO>,
        IAsyncObjectHandler where TO : IPersistentObjectAsync {

        internal AdvancedObjectHandler (string localFilePath, TO[] staticObjects
        ) : base(localFilePath, staticObjects) { }


        internal AdvancedObjectHandler (string localFilePath, Func<TO> factoryFunc
        ) : base(localFilePath, factoryFunc) { }


        public override AdvancedObjectHandler<TO> ResetToDefault () {
            base.ResetToDefault();
            return this;
        }


        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj == null);
            var savedObjects = new List<TO>(dynamicObjects);

            await using UnityWriter unityWriter = UnityHandlersProvider.GetWriter(destinationPath);
            unityWriter.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            HandlingResult result = await InternalHandling.Advanced.TrySaveObjectsAsync(
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

                HandlingResult result = await InternalHandling.Advanced.TryLoadDynamicObjectsAsync(
                    factoryFunc, this, dynamicObjectsCount, unityReader, loadingProgress, token
                );

                if (result != HandlingResult.Success)
                    return result;

                result = await InternalHandling.Advanced.TryLoadStaticObjectsAsync(
                    staticObjects, unityReader, loadingProgress, token
                );

                return result;
            }

            return HandlingResult.FileNotExists;
        }

    }

}