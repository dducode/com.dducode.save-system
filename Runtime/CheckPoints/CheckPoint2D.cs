using UnityEngine;

namespace SaveSystem.CheckPoints {

    [AddComponentMenu("Scripts/Check Point 2D")]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CheckPoint2D : CheckPointBase {

        private void OnTriggerEnter2D (Collider2D other) {
            SaveSystemCore.SaveAtCheckpoint(other);
        }

    }

}