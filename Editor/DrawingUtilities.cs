using System;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    internal static class DrawingUtilities {

        internal static string DrawKeyProperty (
            string key, string title, string buttonTitle, Func<string> generateKeyAction
        ) {
            using var scope = new GUILayout.HorizontalScope();

            key = EditorGUILayout.TextField(title, key, GUILayout.MaxWidth(500));
            bool isNullOrEmpty = string.IsNullOrEmpty(key);
            bool buttonIsPressed = GUILayout.Button(buttonTitle, GUILayout.ExpandWidth(false));

            if (isNullOrEmpty || buttonIsPressed)
                return generateKeyAction();

            return key;
        }

    }

}