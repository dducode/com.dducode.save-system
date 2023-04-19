using System.Threading.Tasks;

namespace SaveSystem {

    public interface IPersistentObjectAsync {

        public Task Save (UnityAsyncWriter writer);
        public Task Load (UnityAsyncReader reader);

    }

}