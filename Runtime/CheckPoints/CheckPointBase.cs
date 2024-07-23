using UnityEngine;

namespace SaveSystemPackage.CheckPoints {

#if IN_UNITY_PACKAGES_PROJECT
    [Icon("Assets/com.dducode.save-system/Editor/Icons/checkpoint_icon.png")]
#else
    [Icon("Packages/com.dducode.save-system/Editor/Icons/checkpoint_icon.png")]
#endif
    [DisallowMultipleComponent]
    public abstract class CheckPointBase : MonoBehaviour {

        private void OnDrawGizmos () {
            Gizmos.DrawIcon(transform.position, "checkpoint_icon.png");
        }

    }

}