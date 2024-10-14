using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Profiles;
using SaveSystemPackage.Providers;
using SaveSystemPackage.SerializableData;
using SaveSystemPackage.Settings;
using SaveSystemPackage.Storages;
using UnityEngine;

#if UNITY_EDITOR
#endif

// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class SaveSystem {

        public static Game Game { get; private set; }
        public static ProfilesManager ProfilesManager { get; private set; }
        public static SystemSettings Settings { get; private set; }
        public static KeyMap KeyMap { get; private set; }
        public static ILogger Logger { get; set; }

        public static bool Initialized { get; private set; }


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


        public static async Task Initialize () {
            try {
                using (SaveSystemSettings settings = SaveSystemSettings.Load()) {
                    Settings = settings;
                    Game = new Game {
                        Serializer = Settings.SharedSerializer,
                        KeyProvider = new KeyStore(KeyMap = KeyMap.PredefinedMap),
                        DataStorage = new FileSystemStorage(
                            Storage.Root, Settings.SharedSerializer.GetFormatCode(), settings.cacheSize
                        )
                    };
                }

                var data = await Game.LoadData<ProfilesManagerData>();
                ProfilesManager = new ProfilesManager(data);
                SetOnExitPlayModeCallback();
                SetPlayerLoop();
                exitCancellation = new CancellationTokenSource();
                Initialized = true;
                Logger.Log(nameof(SaveSystem), "Initialized");
            }
            catch (Exception ex) {
                Logger.LogError(nameof(SaveSystem),
                    "Error while save system initialization. See console for more information"
                );
                Debug.LogException(ex);
            }
        }

    }

}