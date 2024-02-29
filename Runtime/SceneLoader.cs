using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal.Diagnostic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = SaveSystem.Internal.Logger;
using Object = UnityEngine.Object;

namespace SaveSystem {

    public class SceneLoader : MonoBehaviour {

        public string sceneName;
        private readonly List<IRuntimeSerializable> m_serializables = new();
        private readonly List<IAsyncRuntimeSerializable> m_asyncSerializables = new();


        private void OnValidate () {
            if (FindObjectsByType<SceneLoader>(FindObjectsSortMode.None).Length > 1)
                Logger.LogError("More than one scene serialization objects. It's not supported");

            Scene activeScene = SceneManager.GetActiveScene();

            if (string.IsNullOrEmpty(sceneName))
                sceneName = activeScene.name;
        }


        public SceneLoader RegisterSerializable ([NotNull] IRuntimeSerializable serializable) {
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_serializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log($"Serializable object {serializable} was registered in {name}", this);
            return this;
        }


        public SceneLoader RegisterSerializable ([NotNull] IAsyncRuntimeSerializable serializable) {
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            m_asyncSerializables.Add(serializable);
            DiagnosticService.AddObject(serializable);
            Logger.Log($"Serializable object {serializable} was registered in {name}", this);
            return this;
        }


        public SceneLoader RegisterSerializables ([NotNull] IEnumerable<IRuntimeSerializable> serializables) {
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IRuntimeSerializable[] objects = serializables as IRuntimeSerializable[] ?? serializables.ToArray();
            m_serializables.AddRange(objects);
            DiagnosticService.AddObjects(objects);
            Logger.Log($"Serializable objects was registered in {name}", this);
            return this;
        }


        public SceneLoader RegisterSerializables ([NotNull] IEnumerable<IAsyncRuntimeSerializable> serializables) {
            if (serializables == null)
                throw new ArgumentNullException(nameof(serializables));

            IAsyncRuntimeSerializable[] array = serializables as IAsyncRuntimeSerializable[] ?? serializables.ToArray();
            m_asyncSerializables.AddRange(array);
            DiagnosticService.AddObjects(array);
            Logger.Log($"Serializable objects was registered in {name}", this);
            return this;
        }


        public async UniTask<HandlingResult> LoadSceneDataAsync (
            SaveProfile context, CancellationToken token = default
        ) {
            return await SaveSystemCore.LoadSceneDataAsync(this, context, token);
        }


        internal async UniTask SerializeScope (BinaryWriter writer) {
            m_serializables.RemoveAll(serializable =>
                serializable == null || serializable is Object unityObject && unityObject == null
            );

            m_asyncSerializables.RemoveAll(serializable =>
                serializable == null || serializable is Object unityObject && unityObject == null
            );

            foreach (IRuntimeSerializable serializable in m_serializables)
                serializable.Serialize(writer);

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