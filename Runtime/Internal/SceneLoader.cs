using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem.Internal {

    internal static class SceneLoader {

        private static UniTaskCompletionSource<Scene> m_tcs;


        internal static async UniTask LoadSceneAsync (Action sceneLoading) {
            try {
                ExecuteSceneHandling(await WaitForLoading(sceneLoading));
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        internal static async UniTask LoadSceneAsync<TData> (Action sceneLoading, TData data) {
            try {
                ExecuteSceneHandling(await WaitForLoading(sceneLoading), data);
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        internal static async UniTask LoadSceneAsync (Func<UniTask> asyncSceneLoading) {
            ExecuteSceneHandling(await WaitForAsyncLoading(asyncSceneLoading));
        }


        internal static async UniTask LoadSceneAsync<TData> (Func<UniTask> asyncSceneLoading, TData data) {
            ExecuteSceneHandling(await WaitForAsyncLoading(asyncSceneLoading), data);
        }


        private static async UniTask<Scene> WaitForLoading (Action sceneLoading) {
            m_tcs = new UniTaskCompletionSource<Scene>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            sceneLoading();
            return await m_tcs.Task;
        }


        private static async UniTask<Scene> WaitForAsyncLoading (Func<UniTask> asyncSceneLoading) {
            m_tcs = new UniTaskCompletionSource<Scene>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            await asyncSceneLoading();
            return await m_tcs.Task;
        }


        private static void ExecuteSceneHandling (Scene loadedScene) {
            var sceneHandler = SelectSceneHandler<SceneHandler>(loadedScene);

            if (sceneHandler == null) {
                Logger.LogError("There is no target scene handler");
                return;
            }

            sceneHandler.StartScene();
        }


        private static void ExecuteSceneHandling<TData> (Scene loadedScene, TData data) {
            var sceneHandler = SelectSceneHandler<SceneHandler<TData>>(loadedScene);

            if (sceneHandler == null) {
                Logger.LogError("There is no target scene handler");
                return;
            }

            sceneHandler.StartScene(data);
        }


        private static TSceneHandler SelectSceneHandler<TSceneHandler> (Scene loadedScene) {
            return loadedScene
               .GetRootGameObjects()
               .Select(go => go.GetComponent<TSceneHandler>())
               .FirstOrDefault(component => component != null);
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