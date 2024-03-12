using System.IO;
using SaveSystem.Internal;
using UnityEditor;
using UnityEngine;
using File = UnityEngine.Windows.File;

namespace SaveSystem.Editor {

    internal static class EditorResourcesManager {

        private static readonly string ResourcesFolder = Path.Combine("Assets", "Resources");
        private static readonly string SaveSystemFolder = Path.Combine(ResourcesFolder, "Save System");


        internal static TSettings CreateSettings<TSettings> () where TSettings : ScriptableObject {
            CreateSaveSystemFolders();
            string settingsFilePath = Path.Combine(SaveSystemFolder, $"{typeof(TSettings).Name}.asset");

            if (File.Exists(Path.Combine(Application.dataPath, settingsFilePath))) {
                return ResourcesManager.LoadSettings<TSettings>();
            }
            else {
                var settings = ScriptableObject.CreateInstance<TSettings>();
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