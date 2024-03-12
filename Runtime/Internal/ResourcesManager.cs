using System.IO;
using UnityEditor;
using UnityEngine;
using File = UnityEngine.Windows.File;

namespace SaveSystem.Internal {

    internal static class ResourcesManager {

        private static readonly string ResourcesFolder = Path.Combine("Assets", "Resources");
        private static readonly string SaveSystemFolder = Path.Combine(ResourcesFolder, "Save System");


        internal static TSettings CreateSettings<TSettings> () where TSettings : ScriptableObject {
            CreateSaveSystemFolders();
            string settingsFilePath = Path.Combine(SaveSystemFolder, $"{typeof(TSettings).Name}.asset");

            if (File.Exists(Path.Combine(Application.dataPath, settingsFilePath))) {
                return LoadSettings<TSettings>();
            }
            else {
                var settings = ScriptableObject.CreateInstance<TSettings>();
                AssetDatabase.CreateAsset(settings, settingsFilePath);
                AssetDatabase.Refresh();
                return settings;
            }
        }


        internal static TSettings LoadSettings<TSettings> () where TSettings : ScriptableObject {
            return Resources.Load<TSettings>($"Save System/{typeof(TSettings).Name}");
        }


        internal static bool TryLoadSettings<TSettings> (out TSettings settings) where TSettings : ScriptableObject {
            settings = LoadSettings<TSettings>();
            return settings != null;
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