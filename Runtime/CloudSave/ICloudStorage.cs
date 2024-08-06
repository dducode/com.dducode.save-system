using System.Threading.Tasks;

namespace SaveSystemPackage.CloudSave {

    public interface ICloudStorage {

        public Task Push (StorageData data);
        public Task<StorageData> Pull (string fileName);

    }

}