using UnityEngine;

namespace SaveSystemPackage.CheckPoints {

    [AddComponentMenu("Save System/Check Point")]
    [RequireComponent(typeof(SphereCollider))]
    public class CheckPoint : CheckPointBase {

        private void OnTriggerEnter (Collider other) {
            SaveSystem.SaveAtCheckpoint(other);
        }

    }

}