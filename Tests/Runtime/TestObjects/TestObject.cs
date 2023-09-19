using System;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    internal abstract class TestObject : IPersistentObject {
        
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Color color;

        public abstract void Save (UnityWriter writer);
        public abstract void Load (UnityReader reader);


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