using System.IO;
using UnityEditor;

namespace Editor {

    public static class Tools {

        [MenuItem("Tools/Save System/Open Local Storage")]
        public static void OpenLocalStorage () {
            EditorUtility.RevealInFinder(LocalStorage.StoragePath);
        }


        [MenuItem("Tools/Save System/Clear Local Storage")]
        public static void ClearLocalStorage () {
            Directory.Delete(LocalStorage.StoragePath, true);
            EditorUtility.DisplayDialog("Clear Local Storage", "Success", "Ok");
        }


        [MenuItem("Tools/Save System/Open Local Storage", true)]
        public static bool OpenLocalStorageValidate () {
            return Directory.Exists(LocalStorage.StoragePath);
        }


        [MenuItem("Tools/Save System/Clear Local Storage", true)]
        public static bool ClearLocalStorageValidate () {
            return Directory.Exists(LocalStorage.StoragePath);
        }


        [MenuItem("Tools/Save System/Reload Domain")]
        public static void ReloadDomain () {
            EditorUtility.RequestScriptReload();
        }

    }

}