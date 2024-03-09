using System;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    internal static class DrawingUtilities {

        internal static void DrawKeyProperty (
            SerializedProperty keyProperty, string buttonTitle, Func<string> generateKeyAction
        ) {
            using var scope = new GUILayout.HorizontalScope();

            EditorGUILayout.PropertyField(keyProperty);
            bool isNullOrEmpty = string.IsNullOrEmpty(keyProperty.stringValue);
            bool buttonIsPressed = GUILayout.Button(buttonTitle, GUILayout.ExpandWidth(false));

            if (isNullOrEmpty || buttonIsPressed)
                keyProperty.stringValue = generateKeyAction();
        }

    }

}