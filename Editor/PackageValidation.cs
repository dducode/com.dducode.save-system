#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    namespace SaveSystem.Editor {

        public static class PackageValidation {

            public static void AddTagIfNotExists (string tag) {
                var tagManager =
                    new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

                SerializedProperty tagsProp = tagManager.FindProperty("tags");
                var found = false;

                for (var i = 0; i < tagsProp.arraySize; i++) {
                    SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);

                    if (t.stringValue.Equals(tag)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    tagsProp.InsertArrayElementAtIndex(0);
                    SerializedProperty t = tagsProp.GetArrayElementAtIndex(0);
                    t.stringValue = tag;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"Tag {tag} was added in your project");
                }
            }

        }

    }
#endif