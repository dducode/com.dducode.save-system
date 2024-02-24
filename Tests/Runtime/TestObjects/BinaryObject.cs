using SaveSystem.BinaryHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    internal sealed class BinaryObject : TestObject {

        public override void Serialize (BinaryWriter writer) {
            writer.Write(name);
            writer.Write(position);
            writer.Write(rotation);
            writer.Write(color);
        }


        public override void Deserialize (BinaryReader reader) {
            name = reader.ReadString();
            position = reader.Read<Vector3>();
            rotation = reader.Read<Quaternion>();
            color = reader.Read<Color>();
        }

    }

}