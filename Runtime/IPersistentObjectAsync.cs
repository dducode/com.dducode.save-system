using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;

namespace SaveSystem {

    /// <summary>
    /// You can implement it to mark your object as persistent and also implement async handling
    /// </summary>
    public interface IPersistentObjectAsync {

        /// <summary>
        /// It will be called when system will save data
        /// </summary>
        public UniTask Save (UnityWriter writer);


        /// <summary>
        /// It will be called when system will load data
        /// </summary>
        public UniTask Load (UnityReader reader);

    }

}