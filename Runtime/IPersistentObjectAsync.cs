using System.Threading.Tasks;

namespace SaveSystem {

    public interface IPersistentObjectAsync {

        public Task Save (UnityWriter writer);
        public Task Load (UnityReader reader);

    }

}