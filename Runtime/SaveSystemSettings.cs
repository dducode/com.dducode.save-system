using System.IO;
using SaveSystem.Internal;
using UnityEditor;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;
using File = UnityEngine.Windows.File;

namespace SaveSystem {

    public class SaveSystemSettings : ScriptableObject {

        public SaveEvents enabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnExit;
        public LogLevel enabledLogs = LogLevel.Error | LogLevel.Warning;

        [Min(0)]
        [Tooltip(MessageTemplates.SavePeriodTooltip)]
        public float savePeriod = 15;

        [Tooltip(MessageTemplates.IsParallelTooltip)]
        public bool isParallel;

        [Tooltip(MessageTemplates.DataPathTooltip)]
        public string dataPath = "default_data_file.data";

        public string playerTag = "Player";


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