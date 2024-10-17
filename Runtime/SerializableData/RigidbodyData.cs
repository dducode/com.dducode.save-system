using System;
using UnityEngine;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct RigidbodyData : ISaveData {

        public Vector3Data position;
        public QuaternionData rotation;
        public Vector3Data velocity;
        public Vector3Data angularVelocity;
        public bool isKinematic;

        public bool IsEmpty => false;


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