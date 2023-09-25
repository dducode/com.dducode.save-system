using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.InternalServices.Diagnostic;
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
        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj == null);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            var savedObjects = new List<TO>(dynamicObjects);

            await using UnityWriter unityWriter = UnityHandlersFactory.CreateWriter();
            unityWriter.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            Handling.SaveObjects(savedObjects, unityWriter, savingProgress);

            return await Catcher.TryHandle(
                async () => await Storage.SendDataToRemote(destinationPath, unityWriter.GetBuffer(), token),
                "Data sending was cancelled"
            );
        }


        /// <summary>
        /// Call it to start remote objects loading
        /// </summary>
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

            using UnityReader unityReader = UnityHandlersFactory.CreateReader();
            await unityReader.WriteToBufferAsync(data);
            int dynamicObjectsCount = unityReader.ReadInt();

            List<TO> loadedObjects = SpawnObjects(dynamicObjectsCount);
            AddObjects(loadedObjects);
            loadedObjects.AddRange(staticObjects);

            Handling.LoadObjects(loadedObjects, unityReader, loadingProgress);
            return HandlingResult.Success;
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