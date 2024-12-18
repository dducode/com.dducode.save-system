﻿using SaveSystemPackage.Internal.Security;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    internal static class DrawingUtilities {

        internal static string DrawKeyProperty (
            string key, string title, string buttonTitle, bool showPassword
        ) {
            using var scope = new GUILayout.HorizontalScope();

            key = showPassword ? EditorGUILayout.TextField(title, key) : EditorGUILayout.PasswordField(title, key);

            if (string.IsNullOrEmpty(key) || GUILayout.Button(buttonTitle, GUILayout.ExpandWidth(false)))
                return CryptoUtilities.GenerateKey();

            return key;
        }

    }

}