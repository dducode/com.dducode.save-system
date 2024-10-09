using System.IO;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using JsonSerializerSettings = SaveSystemPackage.Settings.JsonSerializerSettings;

namespace SaveSystemPackage.Serialization {

    public class JsonSerializer : ISerializer {

        private readonly Unity.Plastic.Newtonsoft.Json.JsonSerializer m_baseSerializer;


        public JsonSerializer (Unity.Plastic.Newtonsoft.Json.JsonSerializerSettings settings) {
            m_baseSerializer = new Unity.Plastic.Newtonsoft.Json.JsonSerializer {
                Formatting = settings.Formatting,
                DateFormatHandling = settings.DateFormatHandling,
                DateTimeZoneHandling = settings.DateTimeZoneHandling,
                DateParseHandling = settings.DateParseHandling,
                FloatFormatHandling = settings.FloatFormatHandling,
                FloatParseHandling = settings.FloatParseHandling,
                StringEscapeHandling = settings.StringEscapeHandling,
                ReferenceLoopHandling = settings.ReferenceLoopHandling
            };
        }


        public JsonSerializer (JsonSerializerSettings settings) {
            m_baseSerializer = new Unity.Plastic.Newtonsoft.Json.JsonSerializer {
                Formatting = settings.formatting,
                DateFormatHandling = settings.dateFormatHandling,
                DateTimeZoneHandling = settings.dateTimeZoneHandling,
                DateParseHandling = settings.dateParseHandling,
                FloatFormatHandling = settings.floatFormatHandling,
                FloatParseHandling = settings.floatParseHandling,
                StringEscapeHandling = settings.stringEscapeHandling,
                ReferenceLoopHandling = settings.referenceLoopHandling
            };
        }


        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            using var writer = new StringWriter();
            m_baseSerializer.Serialize(writer, data);
            return Encoding.UTF8.GetBytes(writer.ToString());
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            using var reader = new StringReader(Encoding.UTF8.GetString(data));
            return m_baseSerializer.Deserialize<TData>(new JsonTextReader(reader));
        }


        public string GetFormatCode () {
            return "json";
        }

    }

}