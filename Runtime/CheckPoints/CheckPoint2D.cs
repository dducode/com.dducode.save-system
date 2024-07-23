using UnityEngine;

namespace SaveSystemPackage.CheckPoints {

    [AddComponentMenu("Save System/Check Point 2D")]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CheckPoint2D : CheckPointBase {

        private void OnTriggerEnter2D (Collider2D other) {
            SaveSystem.SaveAtCheckpoint(other);
        }

    }

}