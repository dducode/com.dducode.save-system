using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SaveSystem.InternalServices.Diagnostic;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Base class for all object handlers
    /// </summary>
    /// <typeparam name="T"> Type of handler </typeparam>
    /// <typeparam name="TO"> Type of handled objects </typeparam>
    public abstract class AbstractHandler<T, TO> where T : AbstractHandler<T, TO> {

        internal int diagnosticIndex;

        protected readonly string destinationPath;
        protected readonly TO[] staticObjects = Array.Empty<TO>();
        protected readonly List<TO> dynamicObjects = new();

        protected IProgress<float> savingProgress;
        protected IProgress<float> loadingProgress;
        protected Func<TO> factoryFunc;


        protected AbstractHandler (string destinationPath, TO[] staticObjects) {
            this.destinationPath = destinationPath;
            this.staticObjects = staticObjects;
        }


        protected AbstractHandler (string destinationPath, Func<TO> factoryFunc) {
            this.destinationPath = destinationPath;
            this.factoryFunc = factoryFunc;
        }


        /// <summary>
        /// Add a dynamic object that spawned at runtime
        /// </summary>
        /// <param name="obj"> Spawned object </param>
        public T AddObject ([NotNull] TO obj) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            dynamicObjects.Add(obj);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            return (T)this;
        }


        /// <summary>
        /// Add some objects that spawned at runtime
        /// </summary>
        /// <param name="objects"> Spawned objects </param>
        public T AddObjects ([NotNull] ICollection<TO> objects) {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));
            if (objects.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));

            dynamicObjects.AddRange(objects);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            return (T)this;
        }


        /// <summary>
        /// Add function for objects spawn. This is necessary to load dynamic objects
        /// </summary>
        public T SetFuncFactory ([NotNull] Func<TO> factoryFunc) {
            this.factoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc));
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

    }

}