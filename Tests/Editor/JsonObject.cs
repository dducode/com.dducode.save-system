using UnityEngine;

namespace SaveSystem.Tests.Editor {

    public class JsonObject : TestObject {

        public override void Save (UnityWriter writer) {
            writer.Write(this);
        }


        public override void Load (UnityReader reader) {
            var thisObject = reader.ReadObject<JsonObject>();
            name = thisObject.name;
            position = thisObject.position;
            rotation = thisObject.rotation;
            color = thisObject.color;
        }

    }

}