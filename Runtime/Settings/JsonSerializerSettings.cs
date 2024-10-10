using System;
using Unity.Plastic.Newtonsoft.Json;

namespace SaveSystemPackage.Settings {

    [Serializable]
    public class JsonSerializerSettings {

        public Formatting formatting;
        public DateFormatHandling dateFormatHandling;
        public DateTimeZoneHandling dateTimeZoneHandling;
        public DateParseHandling dateParseHandling;
        public FloatFormatHandling floatFormatHandling;
        public FloatParseHandling floatParseHandling;
        public StringEscapeHandling stringEscapeHandling;
        public ReferenceLoopHandling referenceLoopHandling;

    }

}