﻿using SaveSystem.UnityHandlers;

namespace SaveSystem.Tests.TestObjects {

    internal sealed class BinaryObject : TestObject {

        public override void Save (UnityWriter writer) {
            writer.Write(name);
            writer.Write(position);
            writer.Write(rotation);
            writer.Write(color);
        }


        public override void Load (UnityReader reader) {
            name = reader.ReadString();
            position = reader.ReadVector3();
            rotation = reader.ReadRotation();
            color = reader.ReadColor();
        }

    }

}