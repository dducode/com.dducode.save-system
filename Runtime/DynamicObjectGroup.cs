using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SaveSystemPackage.Attributes;
using SaveSystemPackage.Serialization;
using ArgumentException = System.ArgumentException;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SaveSystemPackage {

    /// <summary>
    /// You can create dynamic objects from this and pass all created objects as serializable
    /// </summary>
    /// <seealso cref="DynamicObjectGroup{TDynamic}.CreateObject"/>
    /// <seealso cref="DynamicObjectGroup{TDynamic}.CreateObjects"/>
    public sealed class DynamicObjectGroup<TDynamic> : IRuntimeSerializable {

        public int Version { get; }

        public int Count => m_objects.Count;

        private readonly List<TDynamic> m_objects = new();
        private readonly List<IRuntimeSerializable> m_serializables = new();

        private readonly IObjectFactory<TDynamic> m_factory;
        private readonly ISerializationProvider<ISerializationAdapter<TDynamic>, TDynamic> m_serializationProvider;


        /// <summary>
        /// Creates a group and spawn, saving and loading dynamic objects using it
        /// </summary>
        /// <param name="factory"> A factory for an objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="version"> Version of the objects </param>
        public DynamicObjectGroup ([NotNull] IObjectFactory<TDynamic> factory, int version = 0) {
            Type type = typeof(TDynamic);
            if (!type.IsDefined(typeof(DynamicObjectAttribute), false))
                throw new ArgumentException($"Type {type.Name} is not defined {nameof(DynamicObjectAttribute)}");

            m_factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Version = version;
        }


        /// <summary>
        /// Creates a group and spawn, saving and loading dynamic objects using it
        /// </summary>
        /// <param name="factory"> A factory for an objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="provider"> A provider to get object serialization adapter </param>
        /// <param name="version"> Version of the objects </param>
        public DynamicObjectGroup (
            [NotNull] IObjectFactory<TDynamic> factory,
            [NotNull] ISerializationProvider<ISerializationAdapter<TDynamic>, TDynamic> provider, int version = 0
        ) : this(factory, version) {
            m_serializationProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }


        /// <summary>
        /// Create a new object using the passed factory
        /// </summary>
        /// <returns> A spawned object </returns>
        public TDynamic CreateObject () {
            TDynamic obj = m_factory.CreateObject();
            Add(obj);
            return obj;
        }


        /// <summary>
        /// Create some objects using the passed factory
        /// </summary>
        /// <param name="count"> Number of created objects </param>
        /// <returns> An enumerating of spawned objects </returns>
        public IEnumerable<TDynamic> CreateObjects (int count) {
            if (count <= 0)
                throw new ArgumentException("Objects count cannot be less or equals zero", nameof(count));

            var objects = new TDynamic[count];
            for (var i = 0; i < count; i++)
                objects[i] = m_factory.CreateObject();
            AddRange(objects);
            return objects;
        }


        /// <summary>
        /// Execute an action for all objects
        /// </summary>
        /// <param name="action"> Action to do </param>
        public void DoForAll (Action<TDynamic> action) {
            ClearNullObjects();
            foreach (TDynamic obj in m_objects)
                action(obj);
        }


        /// <summary>
        /// Forget all spawned objects and exclude their from serialization
        /// </summary>
        /// <remarks>
        /// Note that after this action, managed objects may be collect by GC if they are not referenced.
        /// </remarks>
        public void ForgetAllObjects () {
            m_objects.Clear();
            m_serializables.Clear();
        }


        public void Serialize (SaveWriter writer) {
            ClearNullObjects();

            if (m_objects.Count != m_serializables.Count)
                throw new InvalidOperationException("Number of objects does not match serializable components");

            writer.Write(m_objects.Count);
            if (m_objects.Count == 0)
                return;

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Serialize(writer);
        }


        public void Deserialize (SaveReader reader, int previousVersion) {
            ClearNullObjects();

            var count = reader.Read<int>();
            if (count == 0)
                return;

            CreateObjects(count);

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Deserialize(reader, previousVersion);
        }


        public override string ToString () {
            return $"{nameof(DynamicObjectGroup<TDynamic>)}<{typeof(TDynamic).Name}> [objects: {m_objects.Count}]";
        }


        private void Add ([NotNull] TDynamic obj) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            m_objects.Add(obj);

            if (TryGetSerializable(obj, out IRuntimeSerializable serializable)) {
                m_serializables.Add(serializable);
            }
            else {
                var errorMessage =
                    $"Object {obj} is not serializable, besides impossible get adapter to object because get adapter function is not set";
                throw new InvalidOperationException(errorMessage);
            }
        }


        private void AddRange ([NotNull] IEnumerable<TDynamic> objects) {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            foreach (TDynamic obj in objects)
                Add(obj);
        }


        private void ClearNullObjects () {
            m_objects.RemoveAll(obj => obj is Object unityObject && unityObject == null);
            m_serializables.RemoveAll(serializable =>
                serializable is Object unityObject && unityObject == null
            );
            m_serializables.RemoveAll(serializable =>
                serializable is ISerializationAdapter<TDynamic> {Target: Object unityObject} && unityObject == null
            );
        }


        private bool TryGetSerializable (TDynamic obj, out IRuntimeSerializable result) {
            if (obj is IRuntimeSerializable serializable) {
                result = serializable;
                return true;
            }
            else if (m_serializationProvider != null) {
                result = m_serializationProvider.GetAdapter(obj);
                return true;
            }
            else if (obj is ISerializationProvider<ISerializationAdapter<TDynamic>, TDynamic> provider) {
                result = provider.GetAdapter(obj);
                return true;
            }
            else {
                result = null;
                return false;
            }
        }

    }

}