using Cysharp.Threading.Tasks;

namespace SaveSystem {

    public interface ISceneLoader {

        public void LoadScene ();

    }



    public interface IAsyncSceneLoader {

        public UniTask LoadSceneAsync ();

    }

}