using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
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
                exitCancellation = new CancellationTokenSource();
                Logger.Log(nameof(SaveSystem), "Initialized");
            }
            catch (Exception ex) {
                Logger.LogError(nameof(SaveSystem),
                    "Error while save system initialization. See console for more information"
                );
                Debug.LogException(ex);
            }
        }


        /// <inheritdoc cref="LoadSceneAsync(Func{Task},CancellationToken)"/>
        public static async Task LoadSceneAsync (Func<Task> sceneLoading) {
            await LoadSceneAsync(sceneLoading, exitCancellation.Token);
        }


        /// <summary>
        /// Save the game and load a scene
        /// </summary>
        public static async Task LoadSceneAsync (Func<Task> sceneLoading, CancellationToken token) {
            await s_synchronizationPoint.ExecuteTask(async () => await Game.Save(token));
            await SceneLoader.LoadSceneAsync(sceneLoading);
        }


        /// <inheritdoc cref="LoadSceneAsync{TData}(Func{Task},TData,CancellationToken)"/>
        public static async Task LoadSceneAsync<TData> (Func<Task> sceneLoading, TData passedData) {
            await LoadSceneAsync(sceneLoading, passedData, exitCancellation.Token);
        }


        /// <summary>
        /// Save the game and load a scene
        /// </summary>
        public static async Task LoadSceneAsync<TData> (
            Func<Task> sceneLoading, TData passedData, CancellationToken token
        ) {
            await s_synchronizationPoint.ExecuteTask(async () => await Game.Save(token));
            await SceneLoader.LoadSceneAsync(sceneLoading, passedData);
        }


        /// <summary>
        /// Save the game and exit
        /// </summary>
        /// <param name="exitCode"> if the exit code is zero, the game will be saved </param>
        public static async Task ExitGame (int exitCode = 0) {
            s_synchronizationPoint.Clear();
            exitCancellation.Cancel();
            if (exitCode == 0)
                await s_synchronizationPoint.ExecuteTask(async () => await Game.Save(default));

        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit(exitCode);
        #endif
        }


        public static async Task UploadToCloud () {
            await UploadToCloud(exitCancellation.Token);
        }


        public static async Task UploadToCloud (CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                await s_synchronizationPoint.ExecuteTask(async () => await UploadToCloudStorage(token));
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SaveSystem), "Push to cloud canceled");
            }
        }


        public static async Task DownloadFromCloud () {
            await DownloadFromCloud(exitCancellation.Token);
        }


        public static async Task DownloadFromCloud (CancellationToken token) {
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