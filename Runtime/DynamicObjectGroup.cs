using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SaveSystem.Attributes;
using SaveSystem.Internal.Diagnostic;
using ArgumentException = System.ArgumentException;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Object = UnityEngine.Object;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskResult = Cysharp.Threading.Tasks.UniTask<SaveSystem.HandlingResult>;

#else
using TaskResult = System.Threading.Tasks.Task<SaveSystem.Handlers.HandlingResult>;
#endif

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SaveSystem {

    /// <summary>
    /// You can handle <see cref="IRuntimeSerializable">storable objects</see> using this
    /// </summary>
    public sealed class DynamicObjectGroup<TDynamic> : IRuntimeSerializable {

        public bool DontDestroyOnSceneUnload { get; set; }

        internal readonly int diagnosticIndex;

        private readonly List<TDynamic> m_objects = new();
        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly Func<TDynamic> m_factoryFunc;
        private readonly Func<TDynamic, IRuntimeSerializable> m_getAdapter;

        private int ObjectsCount => m_objects.Count;


        /// <summary>
        /// Creates a group that will saving and loading some objects
        /// </summary>
        /// <param name="factoryFunc"> A function for an objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="getAdapter"> A function to get object adapter (optional) </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public DynamicObjectGroup (
            [NotNull] Func<TDynamic> factoryFunc,
            Func<TDynamic, IRuntimeSerializable> getAdapter = null,
            [CallerMemberName] string caller = ""
        ) {
            Type objectsType = typeof(TDynamic);

            if (!objectsType.IsDefined(typeof(DynamicObjectAttribute), false)) {
                throw new ArgumentException(
                    $"Type {objectsType.Name} is not defined {nameof(DynamicObjectAttribute)}"
                );
            }

            if (factoryFunc == null)
                throw new ArgumentNullException(nameof(factoryFunc));

            DiagnosticService.AddMetadata(
                new DynamicObjectGroupMetadata(caller, objectsType, GCHandle.Alloc(this, GCHandleType.Weak))
            );

            diagnosticIndex = DiagnosticService.HandlersCount;
            m_factoryFunc = factoryFunc;
            m_getAdapter = getAdapter;
        }


        /// <summary>
        /// Add a dynamic object that spawned at runtime
        /// </summary>
        /// <param name="obj"> Spawned object </param>
        public DynamicObjectGroup<TDynamic> Add ([NotNull] TDynamic obj) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            m_objects.Add(obj);
            m_serializables.Add(GetSerializable(obj));
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, ObjectsCount);
            return this;
        }


        /// <summary>
        /// Add some objects that spawned at runtime
        /// </summary>
        /// <param name="objects"> Spawned objects </param>
        public DynamicObjectGroup<TDynamic> AddRange ([NotNull] IEnumerable<TDynamic> objects) {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            foreach (TDynamic obj in objects) {
                m_objects.Add(obj);
                m_serializables.Add(GetSerializable(obj));
            }

            DiagnosticService.UpdateObjectsCount(diagnosticIndex, ObjectsCount);
            return this;
        }


        public void Destroy () {
            foreach (TDynamic obj in m_objects) {
                if (obj is Object unityObject)
                    Object.Destroy(unityObject);
            }
        }


        public override string ToString () {
            return $"{nameof(DynamicObjectGroup<TDynamic>)} [objects: {ObjectsCount}]";
        }


        public void Serialize (BinaryWriter writer) {
            m_objects.RemoveAll(obj => obj.Equals(null));
            m_serializables.RemoveAll(serializable => serializable.Equals(null));
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, ObjectsCount);

            writer.Write(m_objects.Count);

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Serialize(writer);
        }


        public void Deserialize (BinaryReader reader) {
            AddRange(CreateObjects(reader.Read<int>()));
            DiagnosticService.UpdateObjectsCount(diagnosticIndex, ObjectsCount);

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Deserialize(reader);
        }


        private IEnumerable<TDynamic> CreateObjects (int count) {
            if (count <= 0)
                return Array.Empty<TDynamic>();

            if (m_factoryFunc == null)
                throw new ArgumentNullException(nameof(m_factoryFunc));

            var objects = new TDynamic[count];
            for (var i = 0; i < count; i++)
                objects[i] = m_factoryFunc();
            return objects;
        }


        private IRuntimeSerializable GetSerializable (TDynamic obj) {
            if (obj is IRuntimeSerializable serializable) {
                return serializable;
            }
            else if (m_getAdapter != null) {
                return m_getAdapter(obj);
            }
            else {
                var errorMessage =
                    $"Object {obj} is not serializable, besides impossible get adapter to object because get adapter function is not set";
                throw new InvalidOperationException(errorMessage);
            }
        }

    }

}