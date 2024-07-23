using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    internal class EditorIconsTool {

        internal static Texture2D GetMainIcon () {
#if IN_UNITY_PACKAGES_PROJECT
            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/com.dducode.save-system/Editor/Icons/checkpoints_manager_icon.png"
            );
#else
            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/com.dducode.save-system/Editor/Icons/checkpoints_manager_icon.png"
            );
#endif
        }

    }

}