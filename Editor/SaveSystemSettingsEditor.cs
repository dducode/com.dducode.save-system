using SaveSystemPackage;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

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