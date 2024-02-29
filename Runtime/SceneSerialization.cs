using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal.Diagnostic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = SaveSystem.Internal.Logger;

namespace SaveSystem {

    public class SceneSerialization : MonoBehaviour {

        public int sceneIndex = -1;
        private readonly List<IRuntimeSerializable> m_serializables = new();
        private Action m_onComplete;
        private bool m_completed;


        private void OnValidate () {
            if (FindObjectsByType<SceneSerialization>(FindObjectsSortMode.None).Length > 1)
                Logger.LogError("More than one scene serialization objects. It's not supported");
            if (sceneIndex == -1)
                sceneIndex = SceneManager.GetActiveScene().buildIndex;
        }


        /// <summary>
        /// Registers an serializable object to serialization
        /// </summary>
        public SceneSerialization RegisterSerializable (
            [NotNull] IRuntimeSerializable serializable, [CallerMemberName] string caller = ""
        ) {
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(serializable);
            DiagnosticService.AddObject(serializable, caller);
            Logger.Log($"Serializable object {serializable} was registered in {name}", this);
            return this;
        }


        /// <summary>
        /// Registers some serializable objects to serialization
        /// </summary>
        public SceneSerialization RegisterSerializables (
            [NotNull] IEnumerable<IRuntimeSerializable> serializables, [CallerMemberName] string caller = ""
        ) {
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] array = serializables.ToArray();
            m_serializables.AddRange(array);
            DiagnosticService.AddObjects(array, caller);
            Logger.Log($"Serializable objects was registered in {name}", this);
            return this;
        }


        public void Complete () {
            m_completed = true;
        }


        internal void Serialize (BinaryWriter writer) {
            m_serializables.RemoveAll(serializable => serializable.Equals(null));
            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Serialize(writer);
        }


        internal void Deserialize (BinaryReader reader) {
            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Deserialize(reader);
        }


        internal async UniTask WaitForComplete () {
            while (!m_completed)
                await UniTask.Yield();
        }

    }

}