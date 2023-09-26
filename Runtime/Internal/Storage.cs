using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SaveSystem.Internal {

    /// <summary>
    /// Use this class to get information about data
    /// </summary>
    public static class Storage {

        private static readonly string PersistentDataPath = Application.persistentDataPath;


        /// <returns> Returns the size of the data in bytes </returns>
        public static long GetDataSize () {
            return GetDataSize(PersistentDataPath);
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
            return GetDataSize(PersistentDataPath) > 0;
        }


        internal static async UniTask<byte[]> GetDataFromRemote (string url, CancellationToken token) {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
            return request.downloadHandler.data;
        }


        internal static async UniTask SendDataToRemote (string url, byte[] data, CancellationToken token) {
            using UnityWebRequest request = UnityWebRequest.Put(url, data);
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
        }


        internal static string GetFullPath (string localFilePath) {
            return Path.Combine(PersistentDataPath, $"{localFilePath}");
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
            const double kByte = 1024;
            const double mByte = 1_048_576;
            const double gByte = 1_073_741_824;
            
            string label;

            switch (dataSize) {
                case < 1_000:
                    label = $"{dataSize} Bytes";
                    break;
                case < 1_000_000:
                    double size = dataSize / kByte;
                    label = $"{size:F} KBytes";
                    break;
                case < 1_000_000_000:
                    size = dataSize / mByte;
                    label = $"{size:F} MBytes";
                    break;
                default:
                    size = dataSize / gByte;
                    label = $"{size:F} GBytes";
                    break;
            }

            return label;
        }


        /// <summary>
        /// It's unsafe calling. Make sure you want it
        /// </summary>
        internal static void DeleteAllData () {
            string[] data = Directory.GetFileSystemEntries(PersistentDataPath);

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