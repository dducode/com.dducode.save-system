using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaveSystem {

    public static class SaveSystemEditor {

        [MenuItem("Save System/Remove Data")]
        private static void RemoveData () {
            var data = Directory.GetFiles(Application.persistentDataPath);

            foreach (var filePath in data)
                File.Delete(filePath);

            const string message = "Data deleted successfully";
            Debug.Log(message);
        }


        [MenuItem("Save System/Get Data Size")]
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


        [MenuItem("Save System/Remove Data", true)]
        private static bool ValidateRemoveData () {
            return Directory.GetFiles(Application.persistentDataPath).Length > 0;
        }

    }

}