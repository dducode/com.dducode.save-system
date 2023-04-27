using Cysharp.Threading.Tasks;

namespace SaveSystem {

    public interface IPersistentObjectAsync {

        public UniTask Save (UnityAsyncWriter asyncWriter);
        public UniTask Load (UnityAsyncReader asyncReader);

    }

}