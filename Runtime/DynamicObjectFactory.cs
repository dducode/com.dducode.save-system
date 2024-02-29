using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SaveSystem.Attributes;
using SaveSystem.Internal.Diagnostic;
using ArgumentException = System.ArgumentException;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SaveSystem {

    /// <summary>
    /// You can group dynamic objects into this and pass group as serializable
    /// </summary>
    /// <seealso cref="DynamicObjectFactory{TDynamic}.CreateObject"/>
    /// <seealso cref="DynamicObjectFactory{TDynamic}.CreateObjects"/>
    public sealed class DynamicObjectFactory<TDynamic> : IRuntimeSerializable {

        public int Count => m_objects.Count;

        private readonly List<TDynamic> m_objects = new();
        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly Func<TDynamic> m_factoryFunc;
        private readonly Func<TDynamic, ISerializationAdapter<TDynamic>> m_getAdapter;


        /// <summary>
        /// Creates a group that will saving and loading some objects
        /// </summary>
        /// <param name="factoryFunc"> A function for an objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="getAdapter"> A function to get object adapter (optional) </param>
        public DynamicObjectFactory (
            [NotNull] Func<TDynamic> factoryFunc,
            Func<TDynamic, ISerializationAdapter<TDynamic>> getAdapter = null
        ) {
            Type objectsType = typeof(TDynamic);

            if (!objectsType.IsDefined(typeof(DynamicObjectAttribute), false)) {
                throw new ArgumentException(
                    $"Type {objectsType.Name} is not defined {nameof(DynamicObjectAttribute)}"
                );
            }

            m_factoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc));
            m_getAdapter = getAdapter;
        }


        /// <summary>
        /// Create a new object using the passed factory function
        /// </summary>
        /// <returns> A spawned object </returns>
        public TDynamic CreateObject () {
            TDynamic obj = m_factoryFunc();
            Add(obj);
            return obj;
        }


        /// <summary>
        /// Create some objects using the passed factory function
        /// </summary>
        /// <param name="count"> Number of created objects </param>
        /// <returns> An enumerating of spawned objects </returns>
        public IEnumerable<TDynamic> CreateObjects (int count) {
            if (count <= 0)
                return Array.Empty<TDynamic>();

            var objects = new TDynamic[count];
            for (var i = 0; i < count; i++)
                objects[i] = m_factoryFunc();
            AddRange(objects);
            return objects;
        }


        public override string ToString () {
            return $"{nameof(DynamicObjectFactory<TDynamic>)}<{typeof(TDynamic).Name}> [objects: {Count}]";
        }


        public void Serialize (BinaryWriter writer) {
            m_objects.RemoveAll(obj => obj == null || obj is Object unityObject && unityObject == null);
            m_serializables.RemoveAll(serializable =>
                serializable == null || serializable is Object unitySerializable && unitySerializable == null
            );

            writer.Write(m_objects.Count);

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Serialize(writer);
        }


        public void Deserialize (BinaryReader reader) {
            IEnumerable<TDynamic> objects = CreateObjects(reader.Read<int>());
            DiagnosticService.AddObjects(objects, m_factoryFunc.Method.Name);

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Deserialize(reader);
        }


        private void Add ([NotNull] TDynamic obj) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            m_objects.Add(obj);
            m_serializables.Add(GetSerializable(obj));
            DiagnosticService.AddObject(obj, m_factoryFunc.Method.Name);
        }


        private void AddRange ([NotNull] IEnumerable<TDynamic> objects) {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            foreach (TDynamic obj in objects)
                Add(obj);
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