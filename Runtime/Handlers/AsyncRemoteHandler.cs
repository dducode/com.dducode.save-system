using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.InternalServices.Diagnostic;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// It's same as <see cref="RemoteHandler{TO}">Remote Handler</see> only for objects that are saved and loaded asynchronously
    /// </summary>
    public class AsyncRemoteHandler<TO> : AbstractHandler<AsyncRemoteHandler<TO>, TO>, IAsyncObjectHandler
        where TO : IPersistentObjectAsync {

        public AsyncRemoteHandler (string destinationPath, TO[] staticObjects) :
            base(destinationPath, staticObjects) { }


        public AsyncRemoteHandler (string destinationPath, Func<TO> factoryFunc) :
            base(destinationPath, factoryFunc) { }


        /// <inheritdoc cref="RemoteHandler{TO}.SaveAsync"/>
        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj == null);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            var savedObjects = new List<TO>(dynamicObjects);

            await using UnityWriter unityWriter = UnityHandlersProvider.GetWriter();
            unityWriter.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            HandlingResult result = await Handling.SaveObjectsAsync(savedObjects, unityWriter, savingProgress, token);

            if (result == HandlingResult.Success) {
                result = await Catcher.TryHandle(
                    async () => await Storage.SendDataToRemote(destinationPath, unityWriter.GetBuffer(), token),
                    "Data sending was cancelled"
                );
            }

            return result;
        }


        /// <inheritdoc cref="RemoteHandler{TO}.LoadAsync"/>
        public async UniTask<HandlingResult> LoadAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            byte[] data = Array.Empty<byte>();

            HandlingResult result = await Catcher.TryHandle(
                async () => data = await Storage.GetDataFromRemote(destinationPath, token),
                "Data getting was canceled"
            );

            if (result != HandlingResult.Success)
                return result;

            using UnityReader unityReader = UnityHandlersProvider.GetReader();
            await unityReader.WriteToBufferAsync(data);
            int dynamicObjectsCount = unityReader.ReadInt();

            result = await Handling.LoadDynamicObjectsAsync(
                factoryFunc, this, dynamicObjectsCount, unityReader, loadingProgress, token
            );

            if (result != HandlingResult.Success)
                return result;

            return await Handling.LoadStaticObjectsAsync(staticObjects, unityReader, loadingProgress, token);
        }

    }

}