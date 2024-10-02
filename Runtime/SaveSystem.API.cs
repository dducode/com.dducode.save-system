using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UnityEngine;
using Logger = SaveSystemPackage.Internal.Logger;

#if UNITY_EDITOR
#endif

// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class SaveSystem {

        public static Game Game { get; private set; }
        // public static ICloudStorage CloudStorage { get; set; }
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
                    Game = new Game();
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


        // public static async Task UploadToCloud () {
        //     CancellationToken token = exitCancellation.Token;
        //
        //     try {
        //         token.ThrowIfCancellationRequested();
        //         await s_synchronizationPoint.ExecuteTask(async () => await UploadToCloudStorage(token));
        //     }
        //     catch (OperationCanceledException) {
        //         Logger.LogWarning(nameof(SaveSystem), "Push to cloud canceled");
        //     }
        // }


        // public static async Task DownloadFromCloud () {
        //     CancellationToken token = exitCancellation.Token;
        //
        //     try {
        //         token.ThrowIfCancellationRequested();
        //         await s_synchronizationPoint.ExecuteTask(async () => await DownloadFromCloudStorage(token));
        //     }
        //     catch (OperationCanceledException) {
        //         Logger.LogWarning(nameof(SaveSystem), "Pull from cloud canceled");
        //     }
        // }

    }

}