using System;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Object Handler can help you to saving/loading <see cref="IPersistentObject">persistent objects</see>.
    /// Also you can set parameters for this as a chain of methods
    /// </summary>
    public sealed class ObjectHandler<TObject> : AbstractHandler<ObjectHandler<TObject>, TObject>, IObjectHandler
        where TObject : IPersistentObject {

        internal ObjectHandler (string localFilePath, TObject[] staticObjects, Func<TObject> factoryFunc)
            : base(localFilePath, staticObjects, factoryFunc) { }


        public void Save () {
            using UnityWriter unityWriter = UnityHandlersFactory.CreateDirectWriter(localFilePath);

            dynamicObjects.RemoveAll(obj => obj == null);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            unityWriter.Write(dynamicObjects.Count);

            Handling.SaveObjects(this, unityWriter, savingProgress);
        }


        public HandlingResult Load () {
            using UnityReader unityReader = UnityHandlersFactory.CreateDirectReader(localFilePath);

            if (unityReader == null)
                return HandlingResult.FileNotExists;

            AddObjects(SpawnObjects(unityReader.ReadInt()));
            Handling.LoadObjects(this, unityReader, loadingProgress);
            return HandlingResult.Success;
        }


        private TObject[] SpawnObjects (int count) {
            if (count > 0 && factoryFunc == null)
                throw new ArgumentException(nameof(factoryFunc));

            var objects = new TObject[count];

            for (var i = 0; i < count; i++)
                objects[i] = factoryFunc();

            return objects;
        }

    }

}