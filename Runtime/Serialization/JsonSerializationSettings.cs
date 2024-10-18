using System;
using Newtonsoft.Json;

namespace SaveSystemPackage.Serialization {

    [Serializable]
    public class JsonSerializationSettings {

        public Formatting Formatting;
        public ReferenceLoopHandling ReferenceLoopHandling;

    }

}