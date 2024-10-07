using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SaveSystemPackage.Serialization {

    public class XmlSerializer : ISerializer {

        public Task<byte[]> Serialize<TData> (TData data, CancellationToken token) where TData : ISaveData {
            token.ThrowIfCancellationRequested();
            using var writer = new StringWriter();
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TData));
            serializer.Serialize(writer, data);
            return Task.FromResult(Encoding.UTF8.GetBytes(writer.ToString()));
        }


        public Task<TData> Deserialize<TData> (byte[] data, CancellationToken token) where TData : ISaveData {
            token.ThrowIfCancellationRequested();
            using var reader = new StringReader(Encoding.UTF8.GetString(data));
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TData));
            return Task.FromResult((TData)serializer.Deserialize(new XmlTextReader(reader)));
        }

    }

}