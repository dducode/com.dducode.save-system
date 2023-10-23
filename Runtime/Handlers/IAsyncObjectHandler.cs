using System.Threading;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskResult = Cysharp.Threading.Tasks.UniTask<SaveSystem.Handlers.HandlingResult>;
#else
using TaskResult = System.Threading.Tasks.Task<SaveSystem.Handlers.HandlingResult>;
#endif

namespace SaveSystem.Handlers {

    /// <summary>
    /// Defines methods for async object handling
    /// </summary>
    public interface IAsyncObjectHandler {

        /// <summary>
        /// Call it to start async objects saving
        /// </summary>
        public TaskResult SaveAsync (CancellationToken token = default);


        /// <summary>
        /// Call it to start async objects loading
        /// </summary>
        public TaskResult LoadAsync (CancellationToken token = default);

    }

}