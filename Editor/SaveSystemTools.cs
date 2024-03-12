﻿using UnityEditor;

namespace SaveSystem.Editor {

    internal static class SaveSystemTools {

        [MenuItem("Assets/Create/Save System/Save System Settings")]
        private static void CreateSettings (MenuCommand menuCommand) {
            var settings = EditorResourcesManager.CreateSettings<SaveSystemSettings>();
            Selection.SetActiveObjectWithContext(settings, settings);
        }

    }

}