using System;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [Serializable]
    public class RigidbodyData : ISaveData {

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public bool isKinematic;


        public static implicit operator RigidbodyData (Rigidbody rigidbody) {
            return new RigidbodyData {
                position = rigidbody.position,
                rotation = rigidbody.rotation,
                velocity = rigidbody.velocity,
                angularVelocity = rigidbody.angularVelocity,
                isKinematic = rigidbody.isKinematic
            };
        }

    }

}