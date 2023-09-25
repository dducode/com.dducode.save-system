using System.Threading;
using Cysharp.Threading.Tasks;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Defines methods for async object handling
    /// </summary>
    public interface IAsyncObjectHandler {

        /// <summary>
        /// Call it to start async objects saving
        /// </summary>
        public UniTask<HandlingResult> SaveAsync (CancellationToken token = default);


        /// <summary>
        /// Call it to start async objects loading
        /// </summary>
        public UniTask<HandlingResult> LoadAsync (CancellationToken token = default);

    }

}