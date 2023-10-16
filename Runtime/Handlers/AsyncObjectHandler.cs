using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// It's same as <see cref="ObjectHandler{TO}">Object Handler</see> but works with objects that are saved and loaded asynchronously
    /// </summary>
    public sealed class AsyncObjectHandler<TObject> : AbstractHandler<AsyncObjectHandler<TObject>, TObject>,
        IAsyncObjectHandler where TObject : IPersistentObjectAsync {

        internal AsyncObjectHandler (string localFilePath, TObject[] staticObjects, Func<TObject> factoryFunc
        ) : base(localFilePath, staticObjects, factoryFunc) { }


        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj.Equals(null));
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);

            await using UnityWriter unityWriter = UnityHandlersFactory.CreateBufferingWriter(localFilePath);
            unityWriter.Write(dynamicObjects.Count);

            HandlingResult result = await Handling.SaveObjectsAsync(this, unityWriter, savingProgress, token);

            if (result == HandlingResult.Success)
                await unityWriter.WriteBufferToFileAsync();

            return result;
        }


        public async UniTask<HandlingResult> LoadAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader unityReader = UnityHandlersFactory.CreateBufferingReader(localFilePath);

            if (await unityReader.ReadFileDataToBufferAsync()) {
                dynamicObjects.RemoveAll(obj => obj.Equals(null));
                DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);

                int dynamicObjectsCount = unityReader.ReadInt();
                if (dynamicObjectsCount > 0 && factoryFunc == null)
                    throw new ArgumentNullException(nameof(factoryFunc));

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