﻿using System.IO;
using UnityEditor;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;
using File = UnityEngine.Windows.File;

namespace SaveSystem.Core {

    public class SaveSystemSettings : ScriptableObject {

        public SaveEvents enabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnExit;

        [Min(0)]
        [Tooltip("It's used into autosave loop to determine saving frequency" +
                 "\nIf it equals 0, saving will be executed at every frame")]
        public float savePeriod = 15;

        [Tooltip("Configure it to set parallel saving handlers" +
                 "\nYou must ensure that your objects are thread safe")]
        public bool isParallel;

        [Tooltip("Enables logs" +
                 "\nIt configures only simple logs, other logs (warnings and errors) will be written to console anyway.")]
        public bool debugEnabled;

        [Tooltip("Determines whether checkpoints will be destroyed after saving")]
        public bool destroyCheckPoints = true;

        public string playerTag = "Player";

        [Tooltip("Configure it to automatically register handlers in the Save System Core after creation")]
        public bool registerImmediately;


    #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void CreateSettings () {
            string resourcesPath = Path.Combine(Application.dataPath, "Resources");
            string settingsFilePath = Path.Combine(resourcesPath, $"{nameof(SaveSystemSettings)}.asset");

            if (!Directory.Exists(resourcesPath)) {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.Refresh();
            }

            if (!File.Exists(settingsFilePath)) {
                AssetDatabase.CreateAsset(
                    CreateInstance<SaveSystemSettings>(), $"Assets/Resources/{nameof(SaveSystemSettings)}.asset"
                );
                AssetDatabase.Refresh();
            }
        }
    #endif

    }

}