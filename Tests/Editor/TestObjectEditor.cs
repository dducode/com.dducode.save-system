using System;
using UnityEngine;

namespace SaveSystem.Tests.Editor {

    public abstract class TestObjectEditor : IPersistentObject {
        
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Color color;

        public abstract void Save (UnityWriter writer);
        public abstract void Load (UnityReader reader);


        protected bool Equals (TestObjectEditor other) {
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