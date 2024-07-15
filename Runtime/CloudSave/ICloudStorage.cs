using Cysharp.Threading.Tasks;

namespace SaveSystem.CloudSave {

    public interface ICloudStorage {

        public void Push (StorageData data);
        public UniTask<StorageData> Pull (StorageData data);

    }

}