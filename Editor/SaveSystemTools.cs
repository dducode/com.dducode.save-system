using UnityEditor;

namespace SaveSystem.Editor {

    internal static class SaveSystemTools {

        [MenuItem("Assets/Create/Save System/Save System Settings")]
        private static void CreateSettings (MenuCommand menuCommand) {
            SaveSystemSettings settings = EditorResourcesManager.CreateSettings();
            Selection.SetActiveObjectWithContext(settings, settings);
        }

    }

}