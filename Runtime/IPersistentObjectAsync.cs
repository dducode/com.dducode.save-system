using Cysharp.Threading.Tasks;

namespace SaveSystem {

    public interface IPersistentObjectAsync {

        public UniTask Save (UnityWriter writer);
        public UniTask Load (UnityReader reader);

    }

}