using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;
using UnityEngine;


namespace SaveSystem {

    /// <summary>
    /// Main class for handling data
    /// </summary>
    public static partial class DataManager {

        private const string ObsoleteMessage =
            "It's the obsolete method and it may be removed later. Use methods of the HandlersProvider class instead this";



        #region Saving

        /// <summary>
        /// Save one object
        /// </summary>
        /// <param name="filePath"> the file where the object data will be saved </param>
        /// <param name="obj"> the object which will be saved </param>
        public static void SaveObject<T> (string filePath, T obj) where T : IPersistentObject {
            SaveObjects(filePath, new[] {obj});
        }


        /// <summary>
        /// Save many objects
        /// </summary>
        /// <param name="filePath"> the file where the object data will be saved </param>
        /// <param name="objects"> the objects which will be saved </param>
        public static void SaveObjects<T> (string filePath, IEnumerable<T> objects)
            where T : IPersistentObject {
            using UnityWriter unityWriter = UnityWriter.GetLocal(filePath);

            foreach (T obj in objects)
                obj.Save(unityWriter);
        }

        #endregion



        #region Loading

        /// <summary>
        /// Load one object
        /// </summary>
        /// <param name="filePath"> the file whereof the object data will be load </param>
        /// <param name="obj"> the object which will be load </param>
        /// <returns> Returns true if there is saved data, otherwise false </returns>
        public static bool LoadObject<T> (string filePath, T obj) where T : IPersistentObject {
            return LoadObjects(filePath, new[] {obj});
        }


        /// <summary>
        /// Load many objects
        /// </summary>
        /// <param name="filePath"> the file whereof the object data will be load </param>
        /// <param name="objects"> the objects which will be load </param>
        /// <returns> Returns true if there is saved data, otherwise false </returns>
        public static bool LoadObjects<T> (string filePath, IEnumerable<T> objects)
            where T : IPersistentObject {
            using UnityReader unityReader = UnityReader.GetLocal(filePath);

            if (unityReader is null)
                return false;

            foreach (T obj in objects)
                obj.Load(unityReader);

            return true;
        }

        #endregion



        #region SavingAsync

        /// <summary>
        /// Save one object async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when saving is canceled </exception>
        /// <inheritdoc cref="SaveObject{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async UniTask SaveObjectAsync<T> (
            string filePath,
            T obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            await SaveObjectsAsync(filePath, new[] {obj}, asyncMode, null, source, onComplete);
        }


        /// <summary>
        /// Save many objects async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when saving is canceled </exception>
        /// <inheritdoc cref="SaveObjects{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async UniTask SaveObjectsAsync<T> (
            string filePath,
            T[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            await using UnityWriter unityWriter = UnityWriter.GetLocal(filePath);

            if (await InternalHandling.TryHandleObjectsAsync(objects, asyncMode, unityWriter, progress, source))
                onComplete?.Invoke();
        }

        #endregion



        #region LoadingAsync

        /// <summary>
        /// Load one object async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when loading is canceled </exception>
        /// <inheritdoc cref="LoadObject{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async UniTask<bool> LoadObjectAsync<T> (
            string filePath,
            T obj,
            AsyncMode asyncMode,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            return await LoadObjectsAsync(filePath, new[] {obj}, asyncMode, null, source, onComplete);
        }


        /// <summary>
        /// Load many objects async
        /// </summary>
        /// <exception cref="OperationCanceledException"> throws when loading is canceled </exception>
        /// <inheritdoc cref="LoadObjects{T}"/>
        [Obsolete(ObsoleteMessage)]
        public static async UniTask<bool> LoadObjectsAsync<T> (
            string filePath,
            T[] objects,
            AsyncMode asyncMode,
            IProgress<float> progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        ) where T : IPersistentObject {
            using UnityReader unityReader = UnityReader.GetLocal(filePath);

            if (unityReader is null) {
                source?.Dispose();
                return false;
            }

            if (await InternalHandling.TryHandleObjectsAsync(objects, asyncMode, unityReader, progress, source)) {
                onComplete?.Invoke();
                return true;
            }

            return false;
        }

        #endregion



        /// <returns> Returns the size of the data in bytes </returns>
        public static long GetDataSize () {
            return GetDataSize(Application.persistentDataPath);
        }


        /// <returns> Returns the formatted total data size </returns>
        /// <example>
        /// "64 Bytes", "10.54 KBytes", "0.93 MBytes"
        /// </example>
        public static string GetFormattedDataSize () {
            return GetFormattedDataSize(GetDataSize());
        }


        /// <returns> True if local storage has any data, otherwise false </returns>
        public static bool HasAnyData () {
            return GetDataSize(Application.persistentDataPath) > 0;
        }


        /// <param name="dataSize"> Size of data to will be formatted </param>
        /// <returns> Returns the formatted data size </returns>
        internal static string GetFormattedDataSize (long dataSize) {
            string label;

            switch (dataSize) {
                case < 1_000:
                    label = $"{Math.Round((double)dataSize, 2)} Bytes";
                    break;
                case < 1_000_000:
                    double size = dataSize / 1024d;
                    label = $"{Math.Round(size, 2)} KBytes";
                    break;
                case < 1_000_000_000:
                    size = dataSize / Math.Pow(1024, 2);
                    label = $"{Math.Round(size, 2)} MBytes";
                    break;
                default:
                    size = dataSize / Math.Pow(1024, 3);
                    label = $"{Math.Round(size, 2)} GBytes";
                    break;
            }

            return label;
        }


        /// <summary>
        /// It's unsafe calling. Make sure you want it
        /// </summary>
        internal static void DeleteAllData () {
            string[] data = Directory.GetFileSystemEntries(Application.persistentDataPath);

            foreach (string filePath in data) {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                else if (Directory.Exists(filePath))
                    Directory.Delete(filePath, true);
            }
        }


        private static long GetDataSize (string path) {
            string[] data = Directory.GetFileSystemEntries(path);
            var dataSize = 0L;

            foreach (string filePath in data) {
                if (Directory.Exists(filePath))
                    dataSize += GetDataSize(filePath);
                else
                    dataSize += new FileInfo(filePath).Length;
            }

            return dataSize;
        }

    }

}