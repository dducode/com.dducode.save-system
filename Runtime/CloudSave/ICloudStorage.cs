using System.Threading.Tasks;

namespace SaveSystemPackage.CloudSave {

    public interface ICloudStorage {

        public Task Upload (StorageData data);
        public Task<StorageData> Download (string fileName);

    }

}