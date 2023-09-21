using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SaveSystem.InternalServices {

    public static class Storage {

        private static readonly string PersistentDataPath = Application.persistentDataPath;


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


        internal static async UniTask<byte[]> GetDataFromRemote (string url) {
            UnityWebRequest request = UnityWebRequest.Get(url);

            try {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    InternalLogger.LogError(request.error);
                    return null;
                }

                return request.downloadHandler.data;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return null;
            }
            finally {
                request.Dispose();
            }
        }


        internal static async UniTask<bool> SendDataToRemote (string url, byte[] data) {
            UnityWebRequest request = UnityWebRequest.Put(url, data);

            try {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    InternalLogger.LogError(request.error);
                    return false;
                }

                return true;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return false;
            }
            finally {
                request.Dispose();
            }
        }


        internal static string GetFullPath (string filePath) {
            return Path.Combine(PersistentDataPath, $"{filePath}");
        }


        internal static void CreateFoldersIfNotExists (string localFilePath) {
            List<string> directoryPaths = localFilePath.Split(new[] {Path.DirectorySeparatorChar}).ToList();
            directoryPaths.RemoveAt(directoryPaths.Count - 1); // last element is a file, not a folder

            var path = "";

            foreach (string directoryPath in directoryPaths) {
                path = Path.Combine(path, directoryPath);
                string fullPath = GetFullPath(path);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
            }
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