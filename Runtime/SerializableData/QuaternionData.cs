using System;
using UnityEngine;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct QuaternionData {

        public float x;
        public float y;
        public float z;
        public float w;


        private QuaternionData (float x, float y, float z, float w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }


        public static implicit operator QuaternionData (Quaternion quaternion) {
            return new QuaternionData(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }


        public static implicit operator Quaternion (QuaternionData data) {
            return new Quaternion(data.x, data.y, data.z, data.w);
        }

    }

}