using System;
using UnityEngine;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public struct ColorData : ISaveData {

        public float r;
        public float g;
        public float b;
        public float a;


        public ColorData (float r, float g, float b, float a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }


        public static implicit operator Color (ColorData data) {
            return new Color(data.r, data.g, data.b, data.a);
        }


        public static implicit operator ColorData (Color color) {
            return new ColorData(color.r, color.g, color.b, color.a);
        }

    }

}