using UnityEngine;

namespace SaveSystem.CheckPoints {

    [AddComponentMenu("Scripts/Check Point 2D")]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CheckPoint2D : CheckPointBase {

        private CircleCollider2D m_collider2D;


        internal override void Enable () {
            m_collider2D.enabled = true;
        }


        internal override void Disable () {
            m_collider2D.enabled = false;
        }


        internal override void Destroy () {
            Destroy(gameObject);
        }


        private void Awake () {
            m_collider2D = GetComponent<CircleCollider2D>();
        }


        private void OnTriggerEnter2D (Collider2D other) {
            SaveSystemCore.SaveAtCheckpoint(this, other);
        }

    }

}