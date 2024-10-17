using System;
using UnityEngine;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct TransformData : ISaveData {

        public Vector3Data position;
        public Vector3Data localPosition;

        public Vector3Data eulerAngles;
        public Vector3Data localEulerAngler;

        public QuaternionData rotation;
        public QuaternionData localRotation;

        public Vector3Data scale;

        public bool IsEmpty => false;


        public static implicit operator TransformData (Transform transform) {
            return new TransformData {
                position = transform.position,
                localPosition = transform.localPosition,
                eulerAngles = transform.eulerAngles,
                localEulerAngler = transform.localEulerAngles,
                rotation = transform.rotation,
                localRotation = transform.localRotation,
                scale = transform.localScale
            };
        }

    }

}