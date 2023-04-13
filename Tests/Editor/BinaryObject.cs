using UnityEngine;

namespace SaveSystem.Tests.Editor {

    public class BinaryObject : TestObject {

        public override void Save (UnityWriter writer) {
            writer.Write(name);
            writer.Write(position);
            writer.Write(rotation);
            writer.Write(color);
        }


        public override void Load (UnityReader reader) {
            name = reader.ReadString();
            position = reader.ReadPosition();
            rotation = reader.ReadRotation();
            color = reader.ReadColor();
        }

    }

}