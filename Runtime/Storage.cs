using System.IO;
using UnityEngine;
using Directory = SaveSystemPackage.Internal.Directory;

// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage {

    /// <summary>
    /// Use this class to get information about data
    /// </summary>
    public static class Storage {

        private const string RootName = "save-system";
        private const string InternalName = ".internal";
        private const string ScenesName = "scenes";
        private const string ProfilesName = "profiles";
        private const string ScreenshotsName = "screenshots";
        private const string TestsName = "tests";

        internal static Directory Root =>
            m_storageDirectory ??= new Directory(RootName, Application.persistentDataPath);

        internal static Directory InternalDirectory =>
            m_internalDirectory ??= Root.CreateDirectory(InternalName);

        internal static Directory ScenesDirectory => m_scenesDirectory ??= Root.CreateDirectory(ScenesName);


        internal static Directory ProfilesDirectory =>
            m_profilesDirectory ??= Root.CreateDirectory(ProfilesName);

        internal static Directory ScreenshotsDirectory =>
            m_screenshotsDirectory ??= Root.CreateDirectory(ScreenshotsName);

        internal static Directory TestsDirectory =>
            m_testsDirectory ??= new Directory(TestsName, Application.temporaryCachePath);

        private static Directory m_storageDirectory;
        private static Directory m_internalDirectory;
        private static Directory m_scenesDirectory;
        private static Directory m_profilesDirectory;
        private static Directory m_screenshotsDirectory;
        private static Directory m_testsDirectory;


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
        internal static void DeleteAllData () {
            Root.Clear();
        }

    }

}