﻿using System;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.Internal.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemPackage.Internal {

    internal static class SceneLoader {

        private static UniTaskCompletionSource<Scene> s_tcs;


        internal static async UniTask LoadSceneAsync (Func<UniTask> asyncSceneLoading) {
            SetupTask();
            ExecuteSceneHandling(await WaitForLoading(asyncSceneLoading));
        }


        internal static async UniTask LoadSceneAsync<TData> (Func<UniTask> asyncSceneLoading, TData data) {
            SetupTask();
            ExecuteSceneHandling(await WaitForLoading(asyncSceneLoading), data);
        }


        private static void SetupTask () {
            s_tcs = new UniTaskCompletionSource<Scene>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private static async UniTask<Scene> WaitForLoading (Func<UniTask> asyncSceneLoading) {
            await asyncSceneLoading();
            return await s_tcs.Task;
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
                s_tcs.TrySetResult(scene);
            }
            finally {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

    }

}