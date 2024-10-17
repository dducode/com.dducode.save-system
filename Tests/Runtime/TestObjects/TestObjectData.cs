using System;
using SaveSystemPackage.SerializableData;
using UnityEngine;

namespace SaveSystemPackage.Tests.TestObjects {

    [Serializable]
    public struct TestObjectData : ISaveData {

        public TransformData transformData;
        public Color color;
        public MeshData meshData;

        public bool IsEmpty => false;

    }

}