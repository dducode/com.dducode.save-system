using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// TODO: add description
    /// </summary>
    public class BufferableObjectHandler<TO> : AbstractHandler<BufferableObjectHandler<TO>, TO>, IAsyncObjectHandler
        where TO : IBufferableObject {

        internal BufferableObjectHandler (string localFilePath, TO[] staticObjects, Func<TO> factoryFunc) :
            base(localFilePath, staticObjects, factoryFunc) { }


        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj == null);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            var savedObjects = new List<TO>(dynamicObjects);

            await using UnityWriter writer = UnityHandlersFactory.CreateWriter(localFilePath);
            writer.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            var buffers = new List<DataBuffer>(savedObjects.Count);
            writer.Write(buffers.Capacity);
            foreach (TO obj in savedObjects)
                buffers.Add(obj.Save());

            foreach (DataBuffer buffer in buffers)
                await writer.WriteAsync(buffer);

            await writer.WriteBufferToFileAsync();

            return HandlingResult.Success;
        }


        public async UniTask<HandlingResult> LoadAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader reader = UnityHandlersFactory.CreateReader(localFilePath);

            if (await reader.ReadFileDataToBufferAsync()) {
                int dynamicObjectsCount = reader.ReadInt();
                if (dynamicObjectsCount > 0 && factoryFunc == null)
                    throw new ArgumentNullException(nameof(factoryFunc));

                int buffersCount = reader.ReadInt();
                var buffers = new List<DataBuffer>(buffersCount);

                for (var i = 0; i < buffersCount; i++)
                    buffers.Add(await reader.ReadDataBufferAsync());

                for (var i = 0; i < dynamicObjectsCount; i++) {
                    TO obj = factoryFunc();
                    obj.Load(buffers[i]);
                    AddObject(obj);
                }

                int bufferIndex = dynamicObjectsCount;

                foreach (TO obj in staticObjects)
                    obj.Load(buffers[bufferIndex++]);

                return HandlingResult.Success;
            }

            return HandlingResult.FileNotExists;
        }

    }

}