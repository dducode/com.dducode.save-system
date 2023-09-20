using System;
using System.Threading;

namespace SaveSystem.Handlers {

    public abstract class AbstractHandler<T> where T : AbstractHandler<T> {

        protected AsyncMode asyncMode;
        protected IProgress<float> savingProgress;
        protected IProgress<float> loadingProgress;
        protected CancellationToken token;


        /// <summary>
        /// Reset all parameters to default values
        /// </summary>
        public T ResetToDefault () {
            asyncMode = AsyncMode.OnPlayerLoop;
            savingProgress = null;
            loadingProgress = null;
            token = default;
            return (T)this;
        }


        /// <summary>
        /// Call it to handle data on player loop
        /// </summary>
        public T OnPlayerLoop () {
            asyncMode = AsyncMode.OnPlayerLoop;
            return (T)this;
        }


        /// <summary>
        /// Call it to handle data on thread pool
        /// </summary>
        public T OnThreadPool () {
            asyncMode = AsyncMode.OnThreadPool;
            return (T)this;
        }


        /// <summary>
        /// You can hand over IProgress object to observe the progress of data handling
        /// </summary>
        /// <param name="progress"> Object that observes of handling progress </param>
        public T ObserveProgress (IProgress<float> progress) {
            savingProgress = progress;
            loadingProgress = progress;
            return (T)this;
        }


        /// <summary>
        /// You can hand over two IProgress objects to observe the progress of loading and saving data separately
        /// </summary>
        /// <param name="savingProgress"> Object that observes of saving progress </param>
        /// <param name="loadingProgress"> Object that observes of loading progress </param>
        public T ObserveProgress (IProgress<float> savingProgress, IProgress<float> loadingProgress) {
            this.savingProgress = savingProgress;
            this.loadingProgress = loadingProgress;
            return (T)this;
        }


        /// <summary>
        /// Set cancellation token source to cancel handling
        /// </summary>
        public T SetCancellationToken (CancellationToken token) {
            this.token = token;
            return (T)this;
        }

    }

}