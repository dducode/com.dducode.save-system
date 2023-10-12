using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SaveSystem.Internal.Diagnostic;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Base class for all object handlers
    /// </summary>
    /// <typeparam name="THandler"> Type of handler </typeparam>
    /// <typeparam name="TObject"> Type of handled objects </typeparam>
    public abstract class AbstractHandler<THandler, TObject> : IEnumerable<TObject>
        where THandler : AbstractHandler<THandler, TObject> {

        internal int diagnosticIndex;

        protected readonly string localFilePath;
        protected readonly TObject[] staticObjects;
        protected readonly List<TObject> dynamicObjects = new();

        protected IProgress<float> savingProgress;
        protected IProgress<float> loadingProgress;
        protected Func<TObject> factoryFunc;

        internal int ObjectsCount => staticObjects.Length + dynamicObjects.Count;


        protected AbstractHandler (string localFilePath, TObject[] staticObjects, Func<TObject> factoryFunc) {
            this.localFilePath = localFilePath;
            this.staticObjects = staticObjects;
            this.factoryFunc = factoryFunc;
        }


        /// <summary>
        /// Add a dynamic object that spawned at runtime
        /// </summary>
        /// <param name="obj"> Spawned object </param>
        public THandler AddObject ([NotNull] TObject obj) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            dynamicObjects.Add(obj);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            return (THandler)this;
        }


        /// <summary>
        /// Add some objects that spawned at runtime
        /// </summary>
        /// <param name="objects"> Spawned objects </param>
        public THandler AddObjects ([NotNull] ICollection<TObject> objects) {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            dynamicObjects.AddRange(objects);
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, staticObjects.Length + dynamicObjects.Count);
            return (THandler)this;
        }


        /// <summary>
        /// Add function for objects spawn. This is necessary to load dynamic objects
        /// </summary>
        public THandler SetFactoryFunc ([NotNull] Func<TObject> factoryFunc) {
            this.factoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc));
            return (THandler)this;
        }


        /// <summary>
        /// You can hand over IProgress object to observe the progress of data handling
        /// </summary>
        /// <param name="progress"> Object that observes of handling progress </param>
        public THandler ObserveProgress (IProgress<float> progress) {
            savingProgress = progress;
            loadingProgress = progress;
            return (THandler)this;
        }


        /// <summary>
        /// You can hand over two IProgress objects to observe the progress of loading and saving data separately
        /// </summary>
        /// <param name="savingProgress"> Object that observes of saving progress </param>
        /// <param name="loadingProgress"> Object that observes of loading progress </param>
        public THandler ObserveProgress (IProgress<float> savingProgress, IProgress<float> loadingProgress) {
            this.savingProgress = savingProgress;
            this.loadingProgress = loadingProgress;
            return (THandler)this;
        }


        public IEnumerator<TObject> GetEnumerator () {
            return new Enumerator(staticObjects, dynamicObjects);
        }


        IEnumerator IEnumerable.GetEnumerator () {
            return GetEnumerator();
        }


        public override string ToString () {
            return $"{typeof(THandler).Name}<{typeof(TObject).Name}> [filePath: {localFilePath}]";
        }


        public TObject this [int index] {
            get {
                if (index < 0)
                    throw new ArgumentOutOfRangeException();

                if (staticObjects != null) {
                    if (index < staticObjects.Length)
                        return staticObjects[index];
                    else if (index < staticObjects.Length + dynamicObjects.Count)
                        return dynamicObjects[index - staticObjects.Length];
                    else
                        throw new ArgumentOutOfRangeException();
                }
                else {
                    if (index < dynamicObjects.Count)
                        return dynamicObjects[index];
                    else
                        throw new ArgumentOutOfRangeException();
                }
            }
        }



        private struct Enumerator : IEnumerator<TObject> {

            private int m_currentObjectIndex;
            private readonly TObject[] m_staticObjects;
            private readonly List<TObject> m_dynamicObjects;

            public TObject Current { get; private set; }

            object IEnumerator.Current => Current;


            internal Enumerator (TObject[] staticObjects, List<TObject> dynamicObjects) {
                m_staticObjects = staticObjects;
                m_dynamicObjects = dynamicObjects;
                m_currentObjectIndex = -1;
                Current = default;
            }


            public void Dispose () {
                Reset();
            }


            public bool MoveNext () {
                m_currentObjectIndex++;

                if (m_staticObjects != null) {
                    if (m_currentObjectIndex < m_staticObjects.Length) {
                        Current = m_staticObjects[m_currentObjectIndex];
                        return true;
                    }
                    else if (m_currentObjectIndex < m_staticObjects.Length + m_dynamicObjects.Count) {
                        Current = m_dynamicObjects[m_currentObjectIndex - m_staticObjects.Length];
                        return true;
                    }
                }
                else {
                    if (m_currentObjectIndex < m_dynamicObjects.Count) {
                        Current = m_dynamicObjects[m_currentObjectIndex];
                        return true;
                    }
                }

                return false;
            }


            public void Reset () {
                m_currentObjectIndex = -1;
                Current = default;
            }

        }

    }

}