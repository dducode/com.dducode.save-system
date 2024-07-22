using System.IO;
using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage {

    /// <summary>
    /// Use this class to get information about data
    /// </summary>
    public static class Storage {

        internal static readonly string StorageDataPath = IniDataPath();


        /// <returns> Returns the size of the data in bytes </returns>
        public static long GetDataSize () {
            return GetDataSize(StorageDataPath);
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
            return GetDataSize(StorageDataPath) > 0;
        }


        internal static string GetFullPath (string filePath) {
            return Path.IsPathRooted(filePath) ? filePath : Path.Combine(StorageDataPath, filePath);
        }


        /// <summary>
        /// Creates new directories if they're not exists and returns full path
        /// </summary>
        internal static string PrepareBeforeUsing (string path) {
            return GetFullPath(path);
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
            string[] data = Directory.GetFileSystemEntries(StorageDataPath);

            foreach (string filePath in data) {
                if (File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
                    Directory.Delete(filePath, true);
                else
                    File.Delete(filePath);
            }
        }


        private static string IniDataPath () {
            string storage = Path.Combine(Application.persistentDataPath, "save-system");
            if (!Directory.Exists(storage))
                Directory.CreateDirectory(storage);
            return storage;
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