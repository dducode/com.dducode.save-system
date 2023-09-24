using System.Threading;
using Cysharp.Threading.Tasks;

namespace SaveSystem.Handlers {

    /// <summary>
    /// TODO: add description
    /// </summary>
    public interface IAsyncObjectHandler {

        /// <summary>
        /// Call it to start async objects saving
        /// </summary>
        /// <param name="token"> TODO: add description </param>
        public UniTask<HandlingResult> SaveAsync (CancellationToken token = default);


        /// <summary>
        /// Call it to start async objects loading
        /// </summary>
        /// <param name="token"> TODO: add description </param>
        public UniTask<HandlingResult> LoadAsync (CancellationToken token = default);

    }

}