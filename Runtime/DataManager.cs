using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SaveSystem {

    public static class DataManager {

        #region Saving

        public static void SaveObject<T> (string fileName, T obj) where T : IPersistentObject {
            if (obj is null) {
                Debug.LogWarning("Object for saving can't be null");
                return;
            }

            SaveObjects(fileName, new[] {obj});
        }


        public static void SaveObjects<T> (string fileName, List<T> objects) where T : IPersistentObject {
            SaveObjects(fileName, objects.ToArray());
        }


        public static void SaveObjects<T> (string fileName, T[] objects) where T : IPersistentObject {
            if (objects.Length is 0) {
                Debug.LogWarning("Objects for saving haven't been transferred");
                return;
            }

            using var unityWriter = GetUnityWriter(fileName);

            foreach (var obj in objects)
                obj.Save(unityWriter);
        }

        #endregion



        #region Loading

        public static bool LoadObject<T> (string fileName, T obj) where T : IPersistentObject {
            if (obj is null) {
                Debug.LogWarning("Object for loading can't be null");
                return false;
            }

            return LoadObjects(fileName, new[] {obj});
        }


        public static bool LoadObjects<T> (string fileName, List<T> objects) where T : IPersistentObject {
            return LoadObjects(fileName, objects.ToArray());
        }


        public static bool LoadObjects<T> (string fileName, T[] objects) where T : IPersistentObject {
            if (objects.Length is 0) {
                Debug.LogWarning("Objects for loading haven't been transferred");
                return false;
            }

            using var unityReader = GetUnityReader(fileName);

            if (unityReader is null)
                return false;

            foreach (var obj in objects)
                obj.Load(unityReader);

            return true;
        }

        #endregion



        #region SavingAsync

        public static async Task SaveObjectsAsync<T> (string fileName, List<T> objects,
            CancellationTokenSource source = null,
            IProgress progress = null,
            Action onComplete = null
        )
            where T : IPersistentObjectAsync {
            await SaveObjectsAsync(fileName, objects.ToArray(), progress, source, onComplete);
        }


        public static async Task SaveObjectsAsync<T> (string fileName, T[] objects,
            IProgress progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        )
            where T : IPersistentObjectAsync {
            if (objects.Length is 0) {
                Debug.LogWarning("Objects for saving haven't been transferred");
                source?.Dispose();
                return;
            }

            using var unityAsyncWriter = GetUnityAsyncWriter(fileName);
            var completedObjects = 0f;
            source ??= new CancellationTokenSource();

            try {
                foreach (var obj in objects) {
                    await obj.Save(unityAsyncWriter);
                    completedObjects++;
                    progress?.Show(completedObjects / objects.Length);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                unityAsyncWriter.Dispose();
                File.Delete(Path.Combine(Application.persistentDataPath, $"{fileName}.bytes"));
                Debug.Log("Object saving canceled. Data file deleted");
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            finally {
                source.Dispose();
            }

            onComplete?.Invoke();
        }

        #endregion



        #region LoadingAsync

        public static async Task<bool> LoadObjectsAsync<T> (string fileName, List<T> objects,
            IProgress progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        )
            where T : IPersistentObjectAsync {
            return await LoadObjectsAsync(fileName, objects.ToArray(), progress, source, onComplete);
        }


        public static async Task<bool> LoadObjectsAsync<T> (string fileName, T[] objects,
            IProgress progress = null,
            CancellationTokenSource source = null,
            Action onComplete = null
        )
            where T : IPersistentObjectAsync {
            if (objects.Length is 0) {
                Debug.LogWarning("Objects for loading haven't been transferred");
                source?.Dispose();
                return false;
            }

            using var unityAsyncReader = GetUnityAsyncReader(fileName);

            if (unityAsyncReader is null) {
                source?.Dispose();
                return false;
            }

            var completedObjects = 0f;
            source ??= new CancellationTokenSource();

            try {
                foreach (var obj in objects) {
                    await obj.Load(unityAsyncReader);
                    completedObjects++;
                    progress?.Show(completedObjects / objects.Length);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                unityAsyncReader.Dispose();
                File.Delete(Path.Combine(Application.persistentDataPath, $"{fileName}.bytes"));
                Debug.Log("Object loading canceled. Data file deleted");
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            finally {
                source.Dispose();
            }

            onComplete?.Invoke();
            return true;
        }

        #endregion



        private static UnityWriter GetUnityWriter (string fileName) {
            var localPath = Path.Combine(Application.persistentDataPath, $"{fileName}.bytes");
            var binaryWriter = new BinaryWriter(File.Open(localPath, FileMode.Create));
            return new UnityWriter(binaryWriter);
        }


        private static UnityAsyncWriter GetUnityAsyncWriter (string fileName) {
            var localPath = Path.Combine(Application.persistentDataPath, $"{fileName}.bytes");
            var binaryWriter = new BinaryWriter(File.Open(localPath, FileMode.Create));
            return new UnityAsyncWriter(binaryWriter);
        }


        private static UnityReader GetUnityReader (string fileName) {
            var localPath = Path.Combine(Application.persistentDataPath, $"{fileName}.bytes");

            if (!File.Exists(localPath))
                return null;

            var binaryReader = new BinaryReader(File.Open(localPath, FileMode.Open));
            return new UnityReader(binaryReader);
        }


        private static UnityAsyncReader GetUnityAsyncReader (string fileName) {
            var localPath = Path.Combine(Application.persistentDataPath, $"{fileName}.bytes");

            if (!File.Exists(localPath))
                return null;

            var binaryReader = new BinaryReader(File.Open(localPath, FileMode.Open));
            return new UnityAsyncReader(binaryReader);
        }


        [MenuItem("Data Manager/Remove Data")]
        private static void RemoveData () {
            var data = Directory.GetFiles(Application.persistentDataPath);

            foreach (var filePath in data)
                File.Delete(filePath);

            Debug.Log("Data deleted successfully");
        }


        [MenuItem("Data Manager/Get Data Size")]
        private static void GetDataSize () {
            var data = Directory.GetFiles(Application.persistentDataPath);
            var dataSize = 0L;

            foreach (var filePath in data)
                dataSize += new FileInfo(filePath).Length;

            const string message = "Size of data: ";

            switch (dataSize) {
                case > 1_000_000_000:
                    var size = Math.Round(dataSize / Math.Pow(1024d, 3), 2);
                    Debug.Log($"{message}{size} GBytes");
                    break;
                case > 1_000_000:
                    size = Math.Round(dataSize / Math.Pow(1024d, 2), 2);
                    Debug.Log($"{message}{size} MBytes");
                    break;
                case > 1_000:
                    size = Math.Round(dataSize / 1024d, 2);
                    Debug.Log($"{message}{size} KBytes");
                    break;
                default:
                    Debug.Log($"{message}{dataSize} Bytes");
                    break;
            }
        }


        [MenuItem("Data Manager/Remove Data", true)]
        private static bool ValidateRemoveData () {
            return Directory.GetFiles(Application.persistentDataPath).Length > 0;
        }

    }

}