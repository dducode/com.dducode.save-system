using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Editor.Tests {

    public class EditorObject : IPersistentObject {

        public string name;
        public int age;
        public bool isAlive;
        public Color color;
        public Vector3 position;
        public Quaternion rotation;


        public void Save (UnityWriter writer) {
            writer.Write(name);
            writer.Write(age);
            writer.Write(isAlive);
            writer.Write(color);
            writer.Write(position);
            writer.Write(rotation);
        }


        public void Load (UnityReader reader) {
            name = reader.ReadString();
            age = reader.ReadInt();
            isAlive = reader.ReadBool();
            color = reader.ReadColor();
            position = reader.ReadVector3();
            rotation = reader.ReadRotation();
        }

    }

}