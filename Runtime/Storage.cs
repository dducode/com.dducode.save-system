using System.Diagnostics;
using SaveSystemPackage.Internal;
using UnityEngine;
using Directory = SaveSystemPackage.Internal.Directory;

// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage {

    /// <summary>
    /// Use this class to get information about data
    /// </summary>
    public static class Storage {

        private const string ScreenshotsName = "screenshots";

        internal static Directory Root =>
            s_root ??= Directory.CreateRoot("save-system", Application.persistentDataPath);

        internal static Directory InternalDirectory => Root.GetOrCreateDirectory(".internal", true);
        internal static Directory ScenesDirectory => Root.GetOrCreateDirectory("scenes");
        internal static Directory ProfilesDirectory => Root.GetOrCreateDirectory("profiles");
        internal static Directory ScreenshotsDirectory => Root.GetOrCreateDirectory(ScreenshotsName);

        internal static Directory TestsDirectory =>
            s_testsDirectory ??= Directory.CreateRoot("tests", Application.temporaryCachePath);

        internal static File HashStorageFile { get; set; }

        private static Directory s_root;
        private static Directory s_internalDirectory;
        private static Directory s_scenesDirectory;
        private static Directory s_profilesDirectory;
        private static Directory s_screenshotsDirectory;
        private static Directory s_testsDirectory;


        /// <returns> Returns the size of the data in bytes </returns>
        public static long GetDataSize () {
            return Root.DataSize;
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
            return Root.DataSize > 0;
        }


        internal static bool ScreenshotsDirectoryExists () {
            return Root.ContainsDirectory(ScreenshotsName);
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
        [Conditional("UNITY_EDITOR")]
        internal static void DeleteAllData () {
            Root.Clear();
        }

    }

}