using UnityEngine;

namespace SaveSystem.Internal {

    internal static class ResourcesManager {

        internal static TSettings LoadSettings<TSettings> () where TSettings : ScriptableObject {
            return Resources.Load<TSettings>($"Save System/{typeof(TSettings).Name}");
        }


        internal static bool TryLoadSettings<TSettings> (out TSettings settings) where TSettings : ScriptableObject {
            settings = LoadSettings<TSettings>();
            return settings != null;
        }

    }

}