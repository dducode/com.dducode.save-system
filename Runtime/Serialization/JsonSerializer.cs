﻿using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using JsonSerializerSettings = SaveSystemPackage.Settings.JsonSerializerSettings;

namespace SaveSystemPackage.Serialization {

    public class JsonSerializer : ISerializer {

        private readonly Unity.Plastic.Newtonsoft.Json.JsonSerializer m_baseSerializer;


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


        public Task<byte[]> Serialize<TData> (TData data, CancellationToken token) where TData : ISaveData {
            token.ThrowIfCancellationRequested();
            using var writer = new StringWriter();
            m_baseSerializer.Serialize(writer, data);
            return Task.FromResult(Encoding.UTF8.GetBytes(writer.ToString()));
        }


        public Task<TData> Deserialize<TData> (byte[] data, CancellationToken token) where TData : ISaveData {
            token.ThrowIfCancellationRequested();
            using var reader = new StringReader(Encoding.UTF8.GetString(data));
            return Task.FromResult(m_baseSerializer.Deserialize<TData>(new JsonTextReader(reader)));
        }

    }

}