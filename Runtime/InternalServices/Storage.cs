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
            return Path.Combine(Application.persistentDataPath, $"{filePath}");
        }

    }

}