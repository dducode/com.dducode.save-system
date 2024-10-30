using SaveSystemPackage.Settings;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomPropertyDrawer(typeof(FileSystemCacheSettings))]
    public class FileSystemCacheSettingsPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            var settings = (FileSystemCacheSettings)property.boxedValue;
            EditorGUILayout.LabelField("File System Cache Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            settings.sizeUnit = (FileSystemCacheSettings.SizeUnit)EditorGUILayout.EnumPopup("Size Unit", settings.sizeUnit);
            settings.cacheSize = EditorGUILayout.IntField("Cache Size", settings.cacheSize);
            EditorGUI.indentLevel--;

            property.boxedValue = settings;
        }

    }

}