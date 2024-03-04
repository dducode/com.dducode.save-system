using System.IO;
using UnityEditor;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;
using File = UnityEngine.Windows.File;

namespace SaveSystem.Editor {

    public static class SaveSystemTools {

        [MenuItem("Assets/Create/Save System/Save System Settings")]
        private static void CreateSettings (MenuCommand menuCommand) {
            SaveSystemSettings settings = CreateSettings();
            Selection.SetActiveObjectWithContext(settings, settings);
        }


        public static SaveSystemSettings CreateSettings () {
            string resourcesPath = Path.Combine(Application.dataPath, "Resources");
            string settingsFilePath = Path.Combine(resourcesPath, $"{nameof(SaveSystemSettings)}.asset");

            if (!Directory.Exists(resourcesPath)) {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.Refresh();
            }

            if (!File.Exists(settingsFilePath)) {
                var settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
                AssetDatabase.CreateAsset(settings, $"Assets/Resources/{nameof(SaveSystemSettings)}.asset");
                AssetDatabase.Refresh();
                return settings;
            }
            else {
                Debug.Log("Save System Settings already exists");
                return Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings));
            }
        }

    }

}