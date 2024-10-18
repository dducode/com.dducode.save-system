using SaveSystemPackage.SerializableData;
using UnityEngine;


namespace SaveSystemPackage.Internal {

    internal static class ResourcesManager {

        internal static KeyMapConfig LoadKeyMapConfig () {
            var asset = Resources.Load<TextAsset>("key-map-config");
            return asset == null ? KeyMapConfig.Empty : SaveSystem.EditorSerializer.Deserialize<KeyMapConfig>(asset.bytes);
        }


        internal static bool KeyMapConfigExists () {
            return Resources.Load<TextAsset>("key-map-config") != null;
        }

    }

}