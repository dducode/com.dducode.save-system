using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using Logger = SaveSystem.Internal.Logger;
using Object = UnityEngine.Object;

// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem.Tasks {

    /// <summary>
    /// Auxiliary class for game loading. You can configure a scene loading action, an after completion action
    /// and also wait for completion
    /// </summary>
    /// <seealso cref="ConfigureSceneLoading(System.Func{int,UnityEngine.AsyncOperation})"/>
    /// <seealso cref="ConfigureSceneLoading(System.Action{int})"/>
    /// <seealso cref="LoadingTask.OnComplete"/>
    /// <seealso cref="LoadingTask.Execute"/>
    /// <seealso cref="LoadingTask.Wait"/>
    public class LoadingTask {

        /// <summary>
        /// Result of the loading
        /// </summary>
        public HandlingResult Result { get; private set; }

        /// <summary>
        /// Exception thrown while loading
        /// </summary>
        public InvalidOperationException Exception { get; private set; }

        /// <summary>
        /// True if the loading was completed with any result, false if the loading is in process
        /// </summary>
        public bool Completed { get; private set; }

        public bool SceneWasLoaded { get; private set; }

        private readonly IEnumerable<IRuntimeSerializable> m_serializableObjects;
        private readonly string m_dataPath;
        private readonly bool m_allowSceneLoading;

        private Func<int, UniTask> m_onSceneLoadingAsync;
        private Action<int> m_onSceneLoading;
        private Action<LoadingTask> m_onComplete;
        private CancellationToken m_token;


        internal LoadingTask (
            IEnumerable<IRuntimeSerializable> serializableObjects, string dataPath, bool allowSceneLoading
        ) {
            m_serializableObjects = serializableObjects;
            m_dataPath = dataPath;
            m_allowSceneLoading = allowSceneLoading;
        }


        /// <inheritdoc cref="ConfigureSceneLoading(System.Action{int})"/>
        public LoadingTask ConfigureSceneLoading (Func<int, AsyncOperation> onSceneLoadingAsyncOperation) {
            m_onSceneLoadingAsync = async sceneIndex => {
                AsyncOperation operation = onSceneLoadingAsyncOperation(sceneIndex);
                while (!operation.isDone)
                    await UniTask.Yield(m_token);
            };
            return this;
        }


        /// <inheritdoc cref="ConfigureSceneLoading(System.Action{int})"/>
        public LoadingTask ConfigureSceneLoading (Func<int, UniTask> onSceneLoadingAsync) {
            m_onSceneLoadingAsync = onSceneLoadingAsync;
            return this;
        }


        /// <summary>
        /// Pass function to loading the last saved scene
        /// </summary>
        public LoadingTask ConfigureSceneLoading (Action<int> onSceneLoading) {
            m_onSceneLoading = onSceneLoading;
            return this;
        }


        /// <summary>
        /// Pass any action that will be called after completion
        /// </summary>
        public LoadingTask OnComplete (Action<LoadingTask> onComplete) {
            m_onComplete = onComplete;
            return this;
        }


        public LoadingTask SetCancellation (CancellationToken token) {
            m_token = token;
            return this;
        }


        /// <summary>
        /// Start the task
        /// </summary>
        public LoadingTask Execute () {
            Result = HandlingResult.InProcess;
            StartLoading();
            return this;
        }


        /// <summary>
        /// Call this to wait for the task to complete
        /// </summary>
        /// <remarks> The task will be start if it hasn't started </remarks>
        public async UniTask<HandlingResult> Wait () {
            if (Result != HandlingResult.InProcess)
                Execute();

            try {
                while (!Completed)
                    await UniTask.Yield(m_token);
                if (Exception != null)
                    throw Exception;
                return Result;
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        private async void StartLoading () {
            if (!File.Exists(m_dataPath)) {
                Complete(HandlingResult.FileNotExists);
                return;
            }

            try {
                m_token.ThrowIfCancellationRequested();
                await UniTask.Yield(m_token);

                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(m_dataPath, m_token);

                foreach (IRuntimeSerializable serializable in m_serializableObjects)
                    serializable.Deserialize(reader);

                if (!m_allowSceneLoading) {
                    Logger.Log("All registered objects was loaded");
                    Complete(HandlingResult.Success);
                    return;
                }

                await LoadSceneAsync(reader.Read<int>());
                SceneWasLoaded = true;

                var sceneSerialization = Object.FindAnyObjectByType<SceneSerialization>();

                if (sceneSerialization == null) {
                    Complete(HandlingResult.Error);
                    Exception = new InvalidOperationException("No scene has a scene serialization object");
                    return;
                }

                await sceneSerialization.WaitForComplete();
                sceneSerialization.Deserialize(reader);

                Logger.Log("All registered objects was loaded");
                Complete(HandlingResult.Success);
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Loading operation was canceled");
                Complete(HandlingResult.Canceled);
            }
        }


        private async UniTask LoadSceneAsync (int sceneIndex) {
            if (m_onSceneLoadingAsync != null) {
                await m_onSceneLoadingAsync(sceneIndex);
            }
            else if (m_onSceneLoading != null) {
                m_onSceneLoading(sceneIndex);
                await UniTask.Yield();
            }
            else {
                SceneManager.LoadScene(sceneIndex);
                await UniTask.Yield();
            }
        }


        private void Complete (HandlingResult result) {
            Completed = true;
            Result = result;
            m_onComplete?.Invoke(this);
        }

    }

}