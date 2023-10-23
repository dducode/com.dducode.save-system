using SaveSystem.UnityHandlers;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAlias = Cysharp.Threading.Tasks.UniTask;
#else
using TaskAlias = System.Threading.Tasks.Task;
#endif

namespace SaveSystem {

    /// <summary>
    /// You can implement it to mark your object as persistent and also implement async handling
    /// </summary>
    public interface IPersistentObjectAsync {

        /// <summary>
        /// It will be called when system will save data
        /// </summary>
        public TaskAlias Save (UnityWriter writer);


        /// <summary>
        /// It will be called when system will load data
        /// </summary>
        public TaskAlias Load (UnityReader reader);

    }

}