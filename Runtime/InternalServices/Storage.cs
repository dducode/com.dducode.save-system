using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SaveSystem.InternalServices {

    internal static class Storage {

        internal static async UniTask<byte[]> GetDataFromRemote (string url) {
            UnityWebRequest request = UnityWebRequest.Get(url);

            try {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError(request.error);
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


        internal static async UniTask SendDataToRemote (string url) {
            byte[] data = await File.ReadAllBytesAsync(GetCachePath());
            UnityWebRequest request = UnityWebRequest.Put(url, data);

            try {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.LogError(request.error);
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
            finally {
                request.Dispose();
                ClearCache();
            }
        }


        internal static string GetFullPath (string filePath) {
            return Path.Combine(Application.persistentDataPath, $"{filePath}");
        }


        internal static string GetCachePath () {
            return Path.Combine(Application.temporaryCachePath, "temp.bytes");
        }


        private static void ClearCache () {
            string[] data = Directory.GetFiles(Application.temporaryCachePath);

            foreach (string file in data)
                File.Delete(file);
        }

    }

}