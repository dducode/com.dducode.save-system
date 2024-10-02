using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;

namespace SaveSystemPackage.Serialization {

    public class JsonSerializer : ISerializer {

        public Task<byte[]> Serialize<TData> (TData data, CancellationToken token) where TData : ISaveData {
            using var writer = new StringWriter();
            var serializer = new Unity.Plastic.Newtonsoft.Json.JsonSerializer();
            serializer.Serialize(writer, data);
            return Task.FromResult(Encoding.UTF8.GetBytes(writer.ToString()));
        }


        public Task<TData> Deserialize<TData> (byte[] data, CancellationToken token) where TData : ISaveData {
            using var reader = new StringReader(Encoding.UTF8.GetString(data));
            var serializer = new Unity.Plastic.Newtonsoft.Json.JsonSerializer();
            return Task.FromResult(serializer.Deserialize<TData>(new JsonTextReader(reader)));
        }

    }

}