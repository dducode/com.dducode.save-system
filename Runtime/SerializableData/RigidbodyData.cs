using System;
using UnityEngine;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public class RigidbodyData : ISaveData {

        public Vector3Data position;
        public QuaternionData rotation;
        public Vector3Data velocity;
        public Vector3Data angularVelocity;
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