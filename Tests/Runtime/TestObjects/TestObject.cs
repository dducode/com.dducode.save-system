using System;
using SaveSystem.BinaryHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    internal abstract class TestObject : IRuntimeSerializable {

        public bool DontDestroyOnSceneUnload => false;

        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Color color;

        public abstract void Serialize (BinaryWriter writer);
        public abstract void Deserialize (BinaryReader reader);


        protected bool Equals (TestObject other) {
            return name == other.name
                   && position.Equals(other.position)
                   && rotation.Equals(other.rotation)
                   && color.Equals(other.color);
        }


        public override int GetHashCode () {
            return HashCode.Combine(name, position, rotation, color);
        }

    }

}