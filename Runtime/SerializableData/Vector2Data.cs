using System;
using UnityEngine;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct Vector2Data : ISaveData {

        public float x;
        public float y;

        public bool IsEmpty => false;


        public Vector2Data (float x, float y) {
            this.x = x;
            this.y = y;
        }


        public static implicit operator Vector2Data (Vector2 vector2) {
            return new Vector2Data(vector2.x, vector2.y);
        }


        public static implicit operator Vector2 (Vector2Data data) {
            return new Vector2(data.x, data.y);
        }

    }

}