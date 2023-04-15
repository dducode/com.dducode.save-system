using UnityEngine;

namespace SaveSystem.Tests.Editor {

    public class JsonObjectEditor : TestObjectEditor {

        public override void Save (UnityWriter writer) {
            writer.Write(this);
        }


        public override void Load (UnityReader reader) {
            var thisObject = reader.ReadObject<JsonObjectEditor>();
            name = thisObject.name;
            position = thisObject.position;
            rotation = thisObject.rotation;
            color = thisObject.color;
        }

    }

}