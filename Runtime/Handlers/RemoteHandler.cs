using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// This handler can help you to saving objects at remote storage and loading them from it
    /// </summary>
    public sealed class RemoteHandler<TO> : AbstractHandler<RemoteHandler<TO>, TO>,
        IAsyncObjectHandler where TO : IPersistentObject {

        internal RemoteHandler (string url, TO[] staticObjects) : base(url, staticObjects) { }


        internal RemoteHandler (string url, Func<TO> factoryFunc) : base(url, factoryFunc) { }


        /// <summary>
        /// Call it to start remote objects saving
        /// </summary>
        /// <param name="token"></param>
        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj == null);
            var savingObjects = new List<TO>(dynamicObjects);

            await using UnityWriter unityWriter = UnityHandlersProvider.GetWriter();
            unityWriter.Write(dynamicObjects.Count);
            savingObjects.AddRange(staticObjects);

            HandlingResult result = await InternalHandling.TrySaveObjectsAsync(
                savingObjects, unityWriter, savingProgress, token
            );

            if (result == HandlingResult.Success) {
                bool requestSucceeded = await Storage.SendDataToRemote(destinationPath, unityWriter.GetBuffer());
                result = requestSucceeded ? HandlingResult.Success : HandlingResult.NetworkError;
            }

            return result;
        }


        /// <summary>
        /// Call it to start remote objects loading
        /// </summary>
        /// <param name="token"></param>
        public async UniTask<HandlingResult> LoadAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            byte[] data = await Storage.GetDataFromRemote(destinationPath);

            if (data is null)
                return HandlingResult.NetworkError;

            using UnityReader unityReader = UnityHandlersProvider.GetReader();
            unityReader.WriteToBuffer(data);

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

    }

}