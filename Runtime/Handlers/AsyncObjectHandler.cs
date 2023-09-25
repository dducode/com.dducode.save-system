using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.InternalServices.Diagnostic;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// It's same as <see cref="ObjectHandler{TO}">Object Handler</see> but works with objects that are saved and loaded asynchronously
    /// </summary>
    public sealed class AsyncObjectHandler<TO> : AbstractHandler<AsyncObjectHandler<TO>, TO>,
        IAsyncObjectHandler where TO : IPersistentObjectAsync {

        internal AsyncObjectHandler (string localFilePath, TO[] staticObjects
        ) : base(localFilePath, staticObjects) { }


        internal AsyncObjectHandler (string localFilePath, Func<TO> factoryFunc
        ) : base(localFilePath, factoryFunc) { }


        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj == null);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            var savedObjects = new List<TO>(dynamicObjects);

            await using UnityWriter unityWriter = UnityHandlersFactory.CreateWriter(destinationPath);
            unityWriter.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            HandlingResult result = await Handling.SaveObjectsAsync(savedObjects, unityWriter, savingProgress, token);

            if (result == HandlingResult.Success)
                await unityWriter.WriteBufferToFileAsync();

            return result;
        }


        public async UniTask<HandlingResult> LoadAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader unityReader = UnityHandlersFactory.CreateReader(destinationPath);

            if (await unityReader.ReadFileDataToBufferAsync()) {
                int dynamicObjectsCount = unityReader.ReadInt();

                HandlingResult result = await Handling.LoadDynamicObjectsAsync(
                    factoryFunc, this, dynamicObjectsCount, unityReader, loadingProgress, token
                );

                if (result != HandlingResult.Success)
                    return result;

                return await Handling.LoadStaticObjectsAsync(staticObjects, unityReader, loadingProgress, token);
            }

            return HandlingResult.FileNotExists;
        }

    }

}