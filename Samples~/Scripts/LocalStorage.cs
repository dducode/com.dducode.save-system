using System.IO;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using UnityEngine;


public class LocalStorage : ICloudStorage {

    public static readonly string StoragePath = Path.Combine(Application.temporaryCachePath, "local-storage");


    public async UniTask Push (StorageData data) {
        Directory.CreateDirectory(StoragePath);
        string path = Path.Combine(StoragePath, data.fileName);
        await File.WriteAllBytesAsync(path, data.rawData);
        Debug.Log($"Push file <b>{data.fileName}</b> to storage");
    }


    public async UniTask<StorageData> Pull (string fileName) {
        if (!Directory.Exists(StoragePath)) {
            Debug.Log("Storage is empty");
            return null;
        }

        string path = Path.Combine(StoragePath, fileName);

        if (!File.Exists(path)) {
            Debug.Log($"Storage doesn't contain file <b>{fileName}</b>");
            return null;
        }

        byte[] bytes = await File.ReadAllBytesAsync(path);
        Debug.Log($"Pull file <b>{fileName}</b> from storage");
        return new StorageData(bytes, fileName);
    }

}