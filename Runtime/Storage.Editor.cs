#if UNITY_EDITOR
    using System.Collections.Generic;
    using SaveSystem.Internal;
    using UnityEditor;

    namespace SaveSystem {

        public static partial class Storage {

            private static List<string> m_storedKeys;


            [InitializeOnLoadMethod]
            private static void InitKeys () {
                m_storedKeys = new List<string>();
                var index = 0;

                while (EditorPrefs.HasKey($"{PrefsKeys.AuthHashKey}_{index}")) {
                    m_storedKeys.Add(EditorPrefs.GetString($"{PrefsKeys.AuthHashKey}_{index}"));
                    ++index;
                }
            }


            internal static void AddPrefsKey (string key) {
                if (!m_storedKeys.Contains(key)) {
                    EditorPrefs.SetString($"{PrefsKeys.AuthHashKey}_{m_storedKeys.Count}", key);
                    m_storedKeys.Add(key);
                }
            }

        }

    }
#endif