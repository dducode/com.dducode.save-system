using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SaveSystem.Internal;
using SaveSystem.Internal.Diagnostic;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;
using Object = UnityEngine.Object;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem {

    public class SaveProfile : IRuntimeSerializable {

        public string Name { get; set; }

        public string DataPath {
            get => m_dataPath;
            set => m_dataPath = Storage.PrepareBeforeUsing(value, true);
        }

        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly List<IAsyncRuntimeSerializable> m_asyncSerializables = new();
        internal int ObjectsCount => m_serializables.Count + m_asyncSerializables.Count;
        private string m_dataPath;


        public void RegisterSerializable ([NotNull] IRuntimeSerializable serializable) {
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log($"Serializable {serializable} was registered in {Name}");
        }


        public void RegisterSerializable ([NotNull] IAsyncRuntimeSerializable serializable) {
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_asyncSerializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log($"Serializable {serializable} was registered in {Name}");
        }


        public void RegisterSerializables ([NotNull] IEnumerable<IRuntimeSerializable> serializables) {
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();
            m_serializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log($"Serializable objects were registered in {Name}");
        }


        public void RegisterSerializables ([NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables) {
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IAsyncRuntimeSerializable[] objects =
                serializables as IAsyncRuntimeSerializable[] ?? serializables.ToArray();
            m_asyncSerializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log($"Serializable objects were registered in {Name}");
        }


        public virtual void Serialize (BinaryWriter writer) {
            writer.Write(Name);
            writer.Write(DataPath);
        }


        public virtual void Deserialize (BinaryReader reader) {
            Name = reader.ReadString();
            DataPath = reader.ReadString();
        }


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.PersistentDataPath, m_dataPath)}";
        }


        internal async UniTask SerializeScope (BinaryWriter writer) {
            m_serializables.RemoveAll(serializable =>
                serializable == null || serializable is Object unityObj && unityObj == null
            );

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Serialize(writer);

            m_asyncSerializables.RemoveAll(serializable =>
                serializable == null || serializable is Object unityObj && unityObj == null
            );

            foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                await serializable.Serialize(writer);
        }


        internal async UniTask DeserializeScope (BinaryReader reader) {
            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Deserialize(reader);

            foreach (IAsyncRuntimeSerializable serializable in m_asyncSerializables)
                await serializable.Deserialize(reader);
        }

    }

}