using UnityEngine;

namespace SaveSystemPackage.CheckPoints {

    [AddComponentMenu("Scripts/Check Point")]
    [RequireComponent(typeof(SphereCollider))]
    public class CheckPoint : CheckPointBase {

        private void OnTriggerEnter (Collider other) {
            SaveSystem.SaveAtCheckpoint(other);
        }

    }

}