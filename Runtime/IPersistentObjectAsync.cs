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
        /// <param name="asyncWriter"> <see cref="UnityAsyncWriter"/> </param>
        public UniTask Save (UnityAsyncWriter asyncWriter);


        /// <summary>
        /// It will be called when system will load data
        /// </summary>
        /// <param name="asyncReader"> <see cref="UnityAsyncReader"/> </param>
        public UniTask Load (UnityAsyncReader asyncReader);

    }

}