using System.IO;
using UnityEngine;

namespace SaveSystem {

    public static partial class DataManager {

        #region Handlers

        private static UnityWriter GetUnityWriter (string fileName) {
            return new UnityWriter(GetBinaryWriter(fileName, out var persistentPath), persistentPath);
        }


        private static UnityAsyncWriter GetUnityAsyncWriter (string fileName) {
            return new UnityAsyncWriter(GetBinaryWriter(fileName, out var persistentPath), persistentPath);
        }


        private static UnityReader GetUnityReader (string fileName) {
            return new UnityReader(GetBinaryReader(fileName));
        }


        private static UnityAsyncReader GetUnityAsyncReader (string fileName) {
            return new UnityAsyncReader(GetBinaryReader(fileName));
        }


        private static BinaryWriter GetBinaryWriter (string fileName, out string persistentPath) {
            persistentPath = GetPersistentPath(fileName);
            return new BinaryWriter(File.Open(persistentPath, FileMode.Create));
        }


        private static BinaryReader GetBinaryReader (string fileName) {
            var persistentPath = GetPersistentPath(fileName);
            return !File.Exists(persistentPath) ? null : new BinaryReader(File.Open(persistentPath, FileMode.Open));
        }

        #endregion



        private static string GetPersistentPath (string fileName) {
            return Path.Combine(Application.persistentDataPath, $"{fileName}.bytes");
        }


        private static string GetTempPath () {
            return Path.Combine(Application.temporaryCachePath, "temp.bytes");
        }

    }

}