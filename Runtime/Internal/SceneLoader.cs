using System;
using System.Threading.Tasks;
using SaveSystemPackage.Internal.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemPackage.Internal {

    internal static class SceneLoader {

        private static TaskCompletionSource<Scene> s_tcs;


        internal static async Task LoadSceneAsync (Func<Task> sceneLoading) {
            SetupTask();
            await sceneLoading();
            SceneHandler handler = await ExecuteSceneHandling(await WaitForCompleteScene());
            handler.StartScene();
        }


        internal static async Task LoadSceneAsync<TData> (Func<Task> sceneLoading, TData data) {
            SetupTask();
            await sceneLoading();
            SceneHandler<TData> handler = await ExecuteSceneHandling<TData>(await WaitForCompleteScene());
            handler.StartScene(data);
        }


        private static void SetupTask () {
            s_tcs = new TaskCompletionSource<Scene>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private static async Task<Scene> WaitForCompleteScene () {
            return await s_tcs.Task;
        }


        private static async Task<SceneHandler> ExecuteSceneHandling (Scene loadedScene) {
            GameObject gameObject = loadedScene.FindWithTag(Tags.SceneHandlerTag);

            if (gameObject == null) {
                Logger.LogError(nameof(SceneLoader), $"There is no target with {Tags.SceneHandlerTag} tag");
                return null;
            }

            var sceneHandler = gameObject.GetComponent<SceneHandler>();

            if (sceneHandler == null) {
                Logger.LogError(nameof(SceneLoader),
                    $"Game object {gameObject.name} has {Tags.SceneHandlerTag} tag, but hasn't {nameof(SceneHandler)} component"
                );
                return null;
            }

            if (sceneHandler.sceneContext != null)
                await sceneHandler.sceneContext.Load();
            return sceneHandler;
        }


        private static async Task<SceneHandler<TData>> ExecuteSceneHandling<TData> (Scene loadedScene) {
            GameObject gameObject = loadedScene.FindWithTag(Tags.SceneHandlerTag);

            if (gameObject == null) {
                Logger.LogError(nameof(SceneLoader), $"There is no target with {Tags.SceneHandlerTag} tag");
                return null;
            }

            var sceneHandler = gameObject.GetComponent<SceneHandler<TData>>();

            if (sceneHandler == null) {
                Logger.LogError(nameof(SceneLoader),
                    $"Game object {gameObject.name} has {Tags.SceneHandlerTag} tag, but hasn't {nameof(SceneHandler<TData>)} component"
                );
                return null;
            }

            if (sceneHandler.sceneContext != null)
                await sceneHandler.sceneContext.Load();
            return sceneHandler;
        }


        private static void OnSceneLoaded (Scene scene, LoadSceneMode loadSceneMode) {
            try {
                s_tcs.TrySetResult(scene);
            }
            finally {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

    }

}