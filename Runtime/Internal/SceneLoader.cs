using System.Threading.Tasks;
using SaveSystemPackage.Internal.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemPackage.Internal {

    internal static class SceneLoader {

        private static TaskCompletionSource<Scene> s_tcs;


        internal static async Task LoadSceneAsync (int index) {
            SetupTask();
            SceneManager.LoadSceneAsync(index);
            SceneHandler handler = await ExecuteSceneHandling(await WaitForLoading());
            handler.StartScene();
        }


        internal static async Task LoadSceneAsync<TData> (int index, TData data) {
            SetupTask();
            SceneManager.LoadSceneAsync(index);
            SceneHandler<TData> handler = await ExecuteSceneHandling<TData>(await WaitForLoading());
            handler.StartScene(data);
        }


        private static void SetupTask () {
            s_tcs = new TaskCompletionSource<Scene>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private static async Task<Scene> WaitForLoading () {
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