using Cysharp.Threading.Tasks;

namespace SaveSystemPackage.Profiles {

    public class SaveProfileRef {

        public string Id { get; }


        public SaveProfileRef (string id) {
            Id = id;
        }


        public async UniTask<SaveProfile> LoadProfile () {
            return await SaveSystem.LoadProfile(Id);
        }

    }

}