using SaveSystemPackage.Providers;
using UnityEngine;

namespace SaveSystemPackage.Internal {

    internal static class KeyProviderFactory {

        public static IKeyProvider Create (KeyMap keyMap) {
            var keyStore = new KeyStore(keyMap);
            if (Debug.isDebugBuild)
                return keyStore;
            else
                return new KeyDecorator(keyStore, "r");
        }

    }

}