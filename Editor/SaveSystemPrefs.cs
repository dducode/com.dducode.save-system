using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    internal static class SaveSystemPrefs {

        internal static void SetColor (string key, Color value) {
            EditorPrefs.SetFloat($"{key}_r", value.r);
            EditorPrefs.SetFloat($"{key}_g", value.g);
            EditorPrefs.SetFloat($"{key}_b", value.b);
            EditorPrefs.SetFloat($"{key}_a", value.a);
        }


        internal static Color GetColor (string key, Color defaultValue) {
            return new Color {
                r = EditorPrefs.GetFloat($"{key}_r", defaultValue.r),
                g = EditorPrefs.GetFloat($"{key}_g", defaultValue.g),
                b = EditorPrefs.GetFloat($"{key}_b", defaultValue.b),
                a = EditorPrefs.GetFloat($"{key}_a", defaultValue.a)
            };
        }

    }

}