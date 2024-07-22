using System.IO;
using SaveSystemPackage;
using SaveSystemPackage.Internal;
using UnityEditor;
using UnityEngine;
using File = UnityEngine.Windows.File;

namespace SaveSystem.Editor {

    internal static class EditorResourcesManager {

        private static readonly string ResourcesFolder = Path.Combine("Assets", "Resources");
        private static readonly string SaveSystemFolder = Path.Combine(ResourcesFolder, "Save System");


        internal static SaveSystemSettings CreateSettings () {
            CreateSaveSystemFolders();
            string settingsFilePath = Path.Combine(SaveSystemFolder, $"{nameof(SaveSystemSettings)}.asset");

            if (File.Exists(Path.Combine(Application.dataPath, settingsFilePath))) {
                return ResourcesManager.LoadSettings();
            }
            else {
                var settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
                AssetDatabase.CreateAsset(settings, settingsFilePath);
                AssetDatabase.Refresh();
                return settings;
            }
        }


        private static void CreateSaveSystemFolders () {
            if (!AssetDatabase.IsValidFolder(SaveSystemFolder)) {
                if (!AssetDatabase.IsValidFolder(ResourcesFolder)) {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateFolder(ResourcesFolder, "Save System");
                AssetDatabase.Refresh();
            }
        }

    }

}