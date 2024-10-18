using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Serialization {

    public class JsonSerializer : ISerializer {

        private readonly JsonSerializationSettings m_settings;


        public JsonSerializer (JsonSerializationSettings settings = null) {
            m_settings = settings ?? new JsonSerializationSettings();
        }


        public byte[] Serialize<TData> ([NotNull] TData data) where TData : ISaveData {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.IsEmpty)
                return Array.Empty<byte>();

            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
                var serializer = new Newtonsoft.Json.JsonSerializer {
                    Formatting = m_settings.Formatting,
                    ReferenceLoopHandling = m_settings.ReferenceLoopHandling
                };
                serializer.Serialize(writer, data);
            }

            return stream.ToArray();
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            if (data == null || data.Length == 0)
                return default;

            using var reader = new StreamReader(new MemoryStream(data), Encoding.UTF8);
            var serializer = new Newtonsoft.Json.JsonSerializer {
                Formatting = m_settings.Formatting,
                ReferenceLoopHandling = m_settings.ReferenceLoopHandling
            };
            return serializer.Deserialize<TData>(new JsonTextReader(reader));
        }


        public string GetFormatCode () {
            return CodeFormats.JSON;
        }

    }

}