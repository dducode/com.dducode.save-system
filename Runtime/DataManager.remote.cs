using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SaveSystem {

    public partial class DataManager {

        #region RemoteHandlers

        private static UnityWriter GetUnityWriterRemote () {
            return new UnityWriter(GetBinaryWriterRemote(out var tempPath), tempPath);
        }


        private static UnityAsyncWriter GetUnityAsyncWriterRemote () {
            return new UnityAsyncWriter(GetBinaryWriterRemote(out var tempPath), tempPath);
        }


        private static async UniTask<UnityReader> GetUnityReaderRemote (string url) {
            var binaryReader = await GetBinaryReaderRemote(url);
            return binaryReader is null ? null : new UnityReader(binaryReader);
        }


        private static async UniTask<UnityAsyncReader> GetUnityAsyncReaderRemote (string url) {
            var binaryReader = await GetBinaryReaderRemote(url);
            return binaryReader is null ? null : new UnityAsyncReader(binaryReader);
        }


        private static BinaryWriter GetBinaryWriterRemote (out string tempPath) {
            tempPath = GetTempPath();
            return new BinaryWriter(File.Open(tempPath, FileMode.Create));
        }


        private static async UniTask<BinaryReader> GetBinaryReaderRemote (string url) {
            var data = await GetDataFromRemote(url);
            return data is null || data.Length is 0 ? null : new BinaryReader(new MemoryStream(data));
        }

        #endregion



        private static async UniTask<byte[]> GetDataFromRemote (string url) {
            var request = UnityWebRequest.Get(url);

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


        private static async UniTask SendDataToRemote (string url) {
            var data = File.ReadAllBytes(GetTempPath());
            var request = UnityWebRequest.Put(url, data);

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
            }
        }


        private static string GetTempPath () {
            return Path.Combine(Application.temporaryCachePath, "temp.bytes");
        }

    }

}