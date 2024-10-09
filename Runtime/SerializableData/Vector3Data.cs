using System;
using UnityEngine;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct Vector3Data : ISaveData {

        public float x;
        public float y;
        public float z;


        public Vector3Data (float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public static implicit operator Vector3 (Vector3Data data) {
            return new Vector3(data.x, data.y, data.z);
        }


        public static implicit operator Vector3Data (Vector3 vector) {
            return new Vector3Data(vector.x, vector.y, vector.z);
        }

    }

}