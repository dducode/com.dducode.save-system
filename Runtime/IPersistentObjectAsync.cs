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
        /// <param name="writer"> <see cref="UnityWriter"/> </param>
        public UniTask Save (UnityWriter writer);


        /// <summary>
        /// It will be called when system will load data
        /// </summary>
        /// <param name="reader"> <see cref="UnityReader"/> </param>
        public UniTask Load (UnityReader reader);

    }

}