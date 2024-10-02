using System;
using UnityEngine;

namespace SaveSystemPackage.ComponentsRecording {

    [Serializable]
    public struct TransformData : ISaveData {

        public Vector3 position;
        public Vector3 localPosition;

        public Vector3 eulerAngles;
        public Vector3 localEulerAngler;

        public Quaternion rotation;
        public Quaternion localRotation;

        public Vector3 scale;


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