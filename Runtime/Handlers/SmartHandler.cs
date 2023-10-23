using System;
using System.Collections.Generic;
using System.Threading;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.UnityHandlers;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskResult = Cysharp.Threading.Tasks.UniTask<SaveSystem.Handlers.HandlingResult>;
#else
using TaskResult = System.Threading.Tasks.Task<SaveSystem.Handlers.HandlingResult>;
#endif

namespace SaveSystem.Handlers {

    /// <summary>
    /// You can handle <see cref="IStorable">storable objects</see> using this
    /// </summary>
    public sealed class SmartHandler<TObject> : AbstractHandler<SmartHandler<TObject>, TObject>, IAsyncObjectHandler
        where TObject : IStorable {

        internal SmartHandler (string localFilePath, TObject[] staticObjects, Func<TObject> factoryFunc) :
            base(localFilePath, staticObjects, factoryFunc) { }


        public async TaskResult SaveAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            dynamicObjects.RemoveAll(obj => obj.Equals(null));
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);

            await using UnityWriter writer = UnityHandlersFactory.CreateDirectWriter(localFilePath);
            writer.Write(dynamicObjects.Count);

            var buffers = new List<DataBuffer>(staticObjects.Length + dynamicObjects.Count);
            writer.Write(buffers.Capacity);
            foreach (TObject obj in this)
                buffers.Add(obj.Save());

            var index = 0;

            foreach (DataBuffer buffer in buffers) {
                await writer.WriteAsync(buffer);
                savingProgress?.Report((float)++index / ObjectsCount);
            }

            return HandlingResult.Success;
        }


        public async TaskResult LoadAsync (CancellationToken token = default) {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader reader = UnityHandlersFactory.CreateDirectReader(localFilePath);

            if (reader == null)
                return HandlingResult.FileNotExists;

            dynamicObjects.RemoveAll(obj => obj.Equals(null));
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);

            int dynamicObjectsCount = reader.ReadInt();

            int buffersCount = reader.ReadInt();
            var buffers = new List<DataBuffer>(buffersCount);

            for (var i = 0; i < buffersCount; i++) {
                buffers.Add(await reader.ReadDataBufferAsync());
                loadingProgress?.Report((float)i / ObjectsCount);
            }

            AddObjects(SpawnObjects(dynamicObjectsCount));

            int count = Math.Min(buffersCount, ObjectsCount);
            for (var i = 0; i < count; i++)
                this[i].Load(buffers[i]);

            return HandlingResult.Success;
        }


        private TObject[] SpawnObjects (int count) {
            if (count > 0 && factoryFunc == null)
                throw new ArgumentNullException(nameof(factoryFunc));

            var objects = new TObject[count];

            for (var i = 0; i < count; i++)
                objects[i] = factoryFunc();

            return objects;
        }

    }

}