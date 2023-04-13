using UnityEngine;

namespace SaveSystem.Tests.Editor {

    public abstract class TestObject : IPersistentObject {
        
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Color color;

        public abstract void Save (UnityWriter writer);
        public abstract void Load (UnityReader reader);

    }

}