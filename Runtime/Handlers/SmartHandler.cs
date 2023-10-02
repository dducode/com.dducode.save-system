using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// You can handle <see cref="IStorable">storable objects</see> using this
    /// </summary>
    public class SmartHandler<TO> : AbstractHandler<SmartHandler<TO>, TO>, IAsyncObjectHandler
        where TO : IStorable {

        internal SmartHandler (string localFilePath, TO[] staticObjects, Func<TO> factoryFunc) :
            base(localFilePath, staticObjects, factoryFunc) { }


        public async UniTask<HandlingResult> SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj == null);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            var savedObjects = new List<TO>(dynamicObjects);

            await using UnityWriter writer = UnityHandlersFactory.CreateDirectWriter(localFilePath);
            writer.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            var buffers = new List<DataBuffer>(savedObjects.Count);
            writer.Write(buffers.Capacity);
            foreach (TO obj in savedObjects)
                buffers.Add(obj.Save());

            foreach (DataBuffer buffer in buffers)
                await writer.WriteAsync(buffer);

            return HandlingResult.Success;
        }


        public async UniTask<HandlingResult> LoadAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader reader = UnityHandlersFactory.CreateDirectReader(localFilePath);

            if (reader != null) {
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