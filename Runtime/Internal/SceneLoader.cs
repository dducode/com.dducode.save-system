using System;
using Cysharp.Threading.Tasks;
using SaveSystem.Internal.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem.Internal {

    internal static class SceneLoader {

        private static UniTaskCompletionSource<Scene> m_tcs;


        internal static async UniTask LoadSceneAsync (Action sceneLoading) {
            try {
                SetupTask();
                ExecuteSceneHandling(await WaitForLoading(sceneLoading));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        internal static async UniTask LoadSceneAsync<TData> (Action sceneLoading, TData data) {
            try {
                SetupTask();
                ExecuteSceneHandling(await WaitForLoading(sceneLoading), data);
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        internal static async UniTask LoadSceneAsync (Func<UniTask> asyncSceneLoading) {
            SetupTask();
            ExecuteSceneHandling(await WaitForAsyncLoading(asyncSceneLoading));
        }


        internal static async UniTask LoadSceneAsync<TData> (Func<UniTask> asyncSceneLoading, TData data) {
            SetupTask();
            ExecuteSceneHandling(await WaitForAsyncLoading(asyncSceneLoading), data);
        }


        private static void SetupTask () {
            m_tcs = new UniTaskCompletionSource<Scene>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private static async UniTask<Scene> WaitForLoading (Action sceneLoading) {
            sceneLoading();
            return await m_tcs.Task;
        }


        private static async UniTask<Scene> WaitForAsyncLoading (Func<UniTask> asyncSceneLoading) {
            await asyncSceneLoading();
            return await m_tcs.Task;
        }


        private static void ExecuteSceneHandling (Scene loadedScene) {
            GameObject gameObject = loadedScene.FindWithTag(Tags.SceneHandlerTag);

            if (gameObject == null) {
                Logger.LogError(nameof(SceneLoader), $"There is no target with {Tags.SceneHandlerTag} tag");
                return;
            }

            var sceneHandler = gameObject.GetComponent<SceneHandler>();

            if (sceneHandler == null) {
                Logger.LogError(nameof(SceneLoader),
                    $"Game object {gameObject.name} has {Tags.SceneHandlerTag} tag, but hasn't {nameof(SceneHandler)} component"
                );
                return;
            }

            sceneHandler.StartScene();
        }


        private static void ExecuteSceneHandling<TData> (Scene loadedScene, TData data) {
            GameObject gameObject = loadedScene.FindWithTag(Tags.SceneHandlerTag);

            if (gameObject == null) {
                Logger.LogError(nameof(SceneLoader), $"There is no target with {Tags.SceneHandlerTag} tag");
                return;
            }

            var sceneHandler = gameObject.GetComponent<SceneHandler<TData>>();

            if (sceneHandler == null) {
                Logger.LogError(nameof(SceneLoader),
                    $"Game object {gameObject.name} has {Tags.SceneHandlerTag} tag, but hasn't {nameof(SceneHandler<TData>)} component"
                );
                return;
            }

            sceneHandler.StartScene(data);
        }


        private static void OnSceneLoaded (Scene scene, LoadSceneMode loadSceneMode) {
            try {
                m_tcs.TrySetResult(scene);
            }
            finally {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

    }

}