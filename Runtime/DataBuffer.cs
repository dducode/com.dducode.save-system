using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SaveSystem {

    /// <summary>
    /// TODO: add description
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct DataBuffer {

        public Vector2 vector2;
        public ReadOnlyMemory<Vector2> vector2Buffer;

        public Vector3 vector3;
        public ReadOnlyMemory<Vector3> vector3Buffer;

        public Vector4 vector4;
        public ReadOnlyMemory<Vector4> vector4Buffer;

        public Quaternion quaternion;

        public Color color;
        public ReadOnlyMemory<Color> colors;

        public Color32 color32;
        public ReadOnlyMemory<Color32> colors32;

        public Matrix4x4 matrix;
        public ReadOnlyMemory<Matrix4x4> matrices;

        public MeshData meshData;

        public ReadOnlyMemory<byte> bytes;
        public ReadOnlyMemory<short> shorts;
        public ReadOnlyMemory<int> intBuffer;
        public ReadOnlyMemory<long> longBuffer;
        public ReadOnlyMemory<char> charBuffer;
        public ReadOnlyMemory<string> stringBuffer;
        public ReadOnlyMemory<float> floatBuffer;
        public ReadOnlyMemory<double> doubleBuffer;
        public bool boolean;


        internal bool HasAnyBuffer () {
            return !vector2Buffer.IsEmpty ||
                   !vector3Buffer.IsEmpty ||
                   !vector4Buffer.IsEmpty ||
                   !colors.IsEmpty ||
                   !colors32.IsEmpty ||
                   !matrices.IsEmpty ||
                   meshData != default ||
                   !bytes.IsEmpty ||
                   !shorts.IsEmpty ||
                   !intBuffer.IsEmpty ||
                   !longBuffer.IsEmpty ||
                   !charBuffer.IsEmpty ||
                   !stringBuffer.IsEmpty ||
                   !floatBuffer.IsEmpty ||
                   !doubleBuffer.IsEmpty;
        }

    }

}