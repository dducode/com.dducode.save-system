﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.CloudSave;
using SaveSystemPackage.Internal;
using UnityEngine;
using Logger = SaveSystemPackage.Internal.Logger;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class SaveSystem {

        public static Game Game { get; private set; }
        public static ICloudStorage CloudStorage { get; set; }
        public static SystemSettings Settings { get; private set; }

        /// <summary>
        /// The event is called before saving. It can be useful when you use async saving
        /// </summary>
        /// <value>
        /// Listeners will be called when the system starts saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveStart;

        /// <summary>
        /// The event is called after saving
        /// </summary>
        /// <value>
        /// Listeners will be called when the system completes saving.
        /// Listeners must accept <see cref="SaveType"/> enumeration
        /// </value>
        public static event Action<SaveType> OnSaveEnd;


        public static void Initialize () {
            try {
                using (SaveSystemSettings settings = SaveSystemSettings.Load()) {
                    SetSettings(settings);
                    Game = new Game(settings);
                }

                SetOnExitPlayModeCallback();
                SetPlayerLoop();
                s_exitCancellation = new CancellationTokenSource();
                Logger.Log(nameof(SaveSystem), "Initialized");
            }
            catch (Exception ex) {
                Logger.LogError(nameof(SaveSystem),
                    "Error while save system initialization. See console for more information"
                );
                Debug.LogException(ex);
            }
        }


        /// <summary>
        /// Save the game and load a scene
        /// </summary>
        public static async UniTask LoadSceneAsync (
            Func<UniTask> sceneLoading, CancellationToken token = default
        ) {
            await s_synchronizationPoint.ExecuteTask(async () => await Game.Save(token));
            await SceneLoader.LoadSceneAsync(sceneLoading);
        }


        /// <summary>
        /// Save the game and load a scene
        /// </summary>
        public static async UniTask LoadSceneAsync<TData> (
            Func<UniTask> sceneLoading, TData passedData, CancellationToken token = default
        ) {
            await s_synchronizationPoint.ExecuteTask(async () => await Game.Save(token));
            await SceneLoader.LoadSceneAsync(sceneLoading, passedData);
        }


        /// <summary>
        /// Save the game and exit
        /// </summary>
        /// <param name="exitCode"> if the exit code is zero, the game will be saved </param>
        public static async UniTask ExitGame (int exitCode = 0) {
            s_synchronizationPoint.Clear();
            s_exitCancellation.Cancel();
            if (exitCode == 0)
                await s_synchronizationPoint.ExecuteTask(async () => await Game.Save());

        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit(exitCode);
        #endif
        }


        public static async UniTask UploadToCloud (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await s_synchronizationPoint.ExecuteTask(async () => await UploadToCloudStorage(token));
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystem), "Push to cloud canceled");
            }
        }


        public static async UniTask DownloadFromCloud (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await s_synchronizationPoint.ExecuteTask(async () => await DownloadFromCloudStorage(token));
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystem), "Pull from cloud canceled");
            }
        }

    }

}