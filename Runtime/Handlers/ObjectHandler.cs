﻿using System;
using System.Collections.Generic;
using SaveSystem.InternalServices;
using SaveSystem.InternalServices.Diagnostic;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Object Handler can help you to saving/loading <see cref="IPersistentObject">persistent objects</see>.
    /// Also you can set parameters for this as a chain of methods
    /// </summary>
    public sealed class ObjectHandler<TO> : AbstractHandler<ObjectHandler<TO>, TO>, IObjectHandler
        where TO : IPersistentObject {

        internal ObjectHandler (string localFilePath, TO[] staticObjects) : base(localFilePath, staticObjects) { }


        internal ObjectHandler (string localFilePath, Func<TO> factoryFunc) : base(localFilePath, factoryFunc) { }


        public void Save () {
            using UnityWriter unityWriter = UnityHandlersFactory.CreateWriter(destinationPath);

            dynamicObjects.RemoveAll(obj => obj == null);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            var savedObjects = new List<TO>(dynamicObjects);
            unityWriter.Write(dynamicObjects.Count);
            savedObjects.AddRange(staticObjects);

            Handling.SaveObjects(savedObjects, unityWriter, savingProgress);
            unityWriter.WriteBufferToFile();
        }


        public HandlingResult Load () {
            using UnityReader unityReader = UnityHandlersFactory.CreateReader(destinationPath);

            if (unityReader.ReadFileDataToBuffer()) {
                int dynamicObjectsCount = unityReader.ReadInt();
                List<TO> loadedObjects = SpawnObjects(dynamicObjectsCount);
                AddObjects(loadedObjects);
                loadedObjects.AddRange(staticObjects);

                Handling.LoadObjects(loadedObjects, unityReader, loadingProgress);
                return HandlingResult.Success;
            }

            return HandlingResult.FileNotExists;
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