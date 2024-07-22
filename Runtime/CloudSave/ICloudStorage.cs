using Cysharp.Threading.Tasks;

namespace SaveSystemPackage.CloudSave {

    public interface ICloudStorage {

        public void Push (StorageData data);
        public UniTask<StorageData> Pull (string fileName);

    }

}