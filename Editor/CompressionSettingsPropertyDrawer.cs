using SaveSystemPackage.Compressing;
using SaveSystemPackage.Settings;
using UnityEditor;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace SaveSystemPackage.Editor {

    [CustomPropertyDrawer(typeof(CompressionSettings))]
    public class CompressionSettingsPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            var settings = (CompressionSettings)property.boxedValue;
            EditorGUILayout.LabelField("Compression Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            settings.useCustomCompressor = EditorGUILayout.Toggle(
                "Use Custom Compressor", settings.useCustomCompressor
            );

            if (settings.useCustomCompressor) {
                EditorGUILayout.ObjectField(
                    "Compressor", settings.reference, typeof(CompressorReference), false
                );
            }
            else {
                settings.compressionLevel = (CompressionLevel)EditorGUILayout.EnumPopup(
                    "Compression Level", settings.compressionLevel
                );
            }

            EditorGUI.indentLevel--;
            property.boxedValue = settings;
        }

    }

}