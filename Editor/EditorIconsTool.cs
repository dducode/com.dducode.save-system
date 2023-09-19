using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    internal class EditorIconsTool {

        internal static Texture2D GetCheckPointsManagerIcon () {
#if IN_UNITY_PACKAGES_PROJECT
            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/com.dducode.save-system/Editor/Icons/checkpoints_manager_icon.png"
            );
#else
            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/Save System/Editor/Icons/checkpoints_manager_icon.png"
            );
#endif
        }

    }

}