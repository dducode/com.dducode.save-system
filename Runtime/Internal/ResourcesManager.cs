using UnityEngine;

namespace SaveSystemPackage.Internal {

    internal static class ResourcesManager {

        internal static SaveSystemSettings LoadSettings () {
            return Resources.Load<SaveSystemSettings>($"Save System/{nameof(SaveSystemSettings)}");
        }


    #if UNITY_EDITOR
        internal static bool TryLoadSettings (out SaveSystemSettings settings) {
            settings = LoadSettings();
            return settings != null;
        }
    #endif

    }

}