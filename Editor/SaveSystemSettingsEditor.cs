using SaveSystemPackage.Settings;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomEditor(typeof(SaveSystemSettings))]
    public class SaveSystemSettingsEditor : UnityEditor.Editor {

        public override void OnInspectorGUI () {
            if (GUILayout.Button("Open In Project Settings")) {
                SettingsService.OpenProjectSettings(SaveSystemSettingsProvider.Path);
                Selection.activeObject = null;
            }
        }

    }

}