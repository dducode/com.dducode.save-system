using SaveSystem.Internal.Cryptography;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    internal class DrawingUtilities {

        internal static void DrawKeyProperty (SerializedProperty keyProperty, string buttonTitle) {
            using var scope = new GUILayout.HorizontalScope();

            EditorGUILayout.PropertyField(keyProperty);    
            bool isNullOrEmpty = string.IsNullOrEmpty(keyProperty.stringValue);
            bool buttonIsPressed = GUILayout.Button(buttonTitle, GUILayout.ExpandWidth(false));

            if (isNullOrEmpty || buttonIsPressed)
                keyProperty.stringValue = CryptoUtilities.GenerateKey();
        }

    }

}