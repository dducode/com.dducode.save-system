using SaveSystem.Core;
using UnityEngine;

namespace SaveSystem.CheckPoints {

    [AddComponentMenu("Scripts/Check Point")]
    [RequireComponent(typeof(SphereCollider))]
    public class CheckPoint : CheckPointBase {

        private SphereCollider m_collider;


        internal override void Enable () {
            m_collider.enabled = true;
        }


        internal override void Disable () {
            m_collider.enabled = false;
        }


        internal override void Destroy () {
            Destroy(gameObject);
        }


        private void Awake () {
            m_collider = GetComponent<SphereCollider>();
        }


        private void OnTriggerEnter (Collider other) {
            SaveSystemCore.SaveAtCheckpoint(this, other);
        }

    }

}