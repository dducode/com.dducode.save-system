using System.IO;
using SaveSystemPackage.Internal;
using SaveSystemPackage.SerializableData;
using SaveSystemPackage.Settings;
using UnityEditor;
using UnityEngine;
using File = UnityEngine.Windows.File;

namespace SaveSystemPackage.Editor {

    internal static class EditorResourcesManager {

        private static readonly string SaveSystemFolder = Path.Combine("Assets", "Save System");
        private static readonly string ResourcesFolder = Path.Combine(SaveSystemFolder, "Resources");


        internal static SaveSystemSettings CreateSettings () {
            CreateSaveSystemFolders();
            string settingsFilePath = Path.Combine(SaveSystemFolder, $"{nameof(SaveSystemSettings)}.asset");

            if (File.Exists(Path.Combine(Application.dataPath, settingsFilePath))) {
                return SaveSystemSettings.Load();
            }
            else {
                var settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
                AssetDatabase.CreateAsset(settings, settingsFilePath);
                AssetDatabase.Refresh();
                return settings;
            }
        }


        [MenuItem("Tools/Save System/Create Key Map Config Template")]
        private static void CreateKeyMapConfigTemplate () {
            byte[] bytes = SaveSystem.EditorSerializer.Serialize(KeyMapConfig.Template);
            string path = Path.Combine(ResourcesFolder, "key-map-config.yaml");
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }


        [MenuItem("Assets/Create/Save System/Save System Settings")]
        private static void CreateSettings (MenuCommand menuCommand) {
            SaveSystemSettings settings = CreateSettings();
            Selection.SetActiveObjectWithContext(settings, settings);
        }


        [MenuItem("Tools/Save System/Create Key Map Config Template", true)]
        private static bool CreateKeyMapConfigTemplateValidate () {
            return !ResourcesManager.KeyMapConfigExists();
        }


        private static void CreateSaveSystemFolders () {
            if (!AssetDatabase.IsValidFolder(SaveSystemFolder)) {
                AssetDatabase.CreateFolder("Assets", "Save System");
                AssetDatabase.Refresh();
            }

            if (!AssetDatabase.IsValidFolder(ResourcesFolder)) {
                AssetDatabase.CreateFolder(SaveSystemFolder, "Resources");
                AssetDatabase.Refresh();
            }
        }

    }

}