using UnityEngine;

namespace SaveSystem.CheckPoints {

    [AddComponentMenu("Scripts/Check Point")]
    [RequireComponent(typeof(SphereCollider))]
    public class CheckPoint : CheckPointBase {

        private void OnTriggerEnter (Collider other) {
            SaveSystemCore.SaveAtCheckpoint(other);
        }

    }

}