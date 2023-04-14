using System.Collections.Generic;
using System.IO;
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



        private static UnityWriter GetUnityWriter (string fileName) {
            var localPath = Path.Combine(Application.persistentDataPath, $"{fileName}.bytes");
            var binaryWriter = new BinaryWriter(File.Open(localPath, FileMode.OpenOrCreate));
            return new UnityWriter(binaryWriter);
        }


        private static UnityReader GetUnityReader (string fileName) {
            var localPath = Path.Combine(Application.persistentDataPath, $"{fileName}.bytes");

            if (!File.Exists(localPath))
                return null;

            var binaryReader = new BinaryReader(File.Open(localPath, FileMode.Open));
            return new UnityReader(binaryReader);
        }


        [MenuItem("Data Manager/Remove Data")]
        private static void RemoveData () {
            var data = Directory.GetFiles(Application.persistentDataPath);

            foreach (var filePath in data)
                File.Delete(filePath);

            Debug.Log("Data deleted successfully");
        }


        [MenuItem("Data Manager/Remove Data", true)]
        private static bool ValidateRemoveData () {
            return Directory.GetFiles(Application.persistentDataPath).Length > 0;
        }

    }

}