using UnityEngine;

namespace SaveSystem.CheckPoints {

#if IN_UNITY_PACKAGES_PROJECT
    [Icon("Assets/com.dducode.save-system/Editor/Icons/checkpoint_icon.png")]
#else
    [Icon("Packages/Save System/Editor/Icons/checkpoint_icon.png")]
#endif
    [DisallowMultipleComponent]
    public abstract class CheckPointBase : MonoBehaviour {

        internal abstract void Enable ();
        internal abstract void Disable ();
        internal abstract void Destroy ();


        private void OnDrawGizmos () {
            Gizmos.DrawIcon(transform.position, "checkpoint_icon.png");
        }

    }

}