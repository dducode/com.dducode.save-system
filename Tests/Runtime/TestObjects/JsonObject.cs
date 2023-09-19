using SaveSystem.UnityHandlers;

namespace SaveSystem.Tests.TestObjects {

    internal sealed class JsonObject : TestObject {

        public override void Save (UnityWriter writer) {
            writer.WriteObject(this);
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