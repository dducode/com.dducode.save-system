using System.IO;
using System.Text;
using System.Xml;

namespace SaveSystemPackage.Serialization {

    public class XmlSerializer : ISerializer {

        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            using var writer = new StringWriter();
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TData));
            serializer.Serialize(writer, data);
            return Encoding.UTF8.GetBytes(writer.ToString());
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            using var reader = new StringReader(Encoding.UTF8.GetString(data));
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TData));
            return (TData)serializer.Deserialize(new XmlTextReader(reader));
        }


        public string GetFormatCode () {
            return "xml";
        }

    }

}