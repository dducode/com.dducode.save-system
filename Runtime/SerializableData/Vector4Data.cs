using System;
using UnityEngine;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct Vector4Data : ISaveData {

        public float x;
        public float y;
        public float z;
        public float w;


        public Vector4Data (float x, float y, float z, float w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }


        public static implicit operator Vector4 (Vector4Data data) {
            return new Vector4(data.x, data.y, data.z, data.w);
        }


        public static implicit operator Vector4Data (Vector4 vector) {
            return new Vector4Data(vector.x, vector.y, vector.z, vector.w);
        }

    }

}