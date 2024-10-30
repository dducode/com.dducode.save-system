using Newtonsoft.Json;
using SaveSystemPackage.Serialization;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomPropertyDrawer(typeof(JsonSerializationSettings))]
    public class JsonSerializationSettingsPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            var settings = (JsonSerializationSettings)property.boxedValue;
            EditorGUILayout.LabelField("Json Serialization Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            settings.Formatting = (Formatting)EditorGUILayout.EnumPopup("Formatting", settings.Formatting);
            settings.ReferenceLoopHandling = (ReferenceLoopHandling)EditorGUILayout.EnumPopup(
                "Reference Loop Handling", settings.ReferenceLoopHandling
            );
            EditorGUI.indentLevel--;

            property.boxedValue = settings;
        }

    }

}