#if UNITY_EDITOR
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    namespace SaveSystem {

        public static partial class Storage {

            private const string AuthHashKeys = "save_system_auth_hash_keys";
            private static List<string> m_storedKeys;


            [InitializeOnLoadMethod]
            private static void InitKeys () {
                m_storedKeys = new List<string>();
                var index = 0;

                while (EditorPrefs.HasKey($"{AuthHashKeys}_{index}")) {
                    m_storedKeys.Add(EditorPrefs.GetString($"{AuthHashKeys}_{index}"));
                    ++index;
                }
            }


            internal static void AddPrefsKey (string key) {
                if (!m_storedKeys.Contains(key)) {
                    EditorPrefs.SetString($"{AuthHashKeys}_{m_storedKeys.Count}", key);
                    m_storedKeys.Add(key);
                }
            }


            private static void DeleteAuthKeys () {
                foreach (string key in m_storedKeys)
                    PlayerPrefs.DeleteKey(key);

                var index = 0;

                while (EditorPrefs.HasKey($"{AuthHashKeys}_{index}")) {
                    EditorPrefs.DeleteKey($"{AuthHashKeys}_{index}");
                    ++index;
                }

                m_storedKeys.Clear();
            }

        }

    }
#endif