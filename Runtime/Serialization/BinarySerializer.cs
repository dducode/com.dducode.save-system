using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SaveSystemPackage.Serialization {

    public class BinarySerializer : ISerializer {

        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            using var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            using var stream = new MemoryStream(data);
            var formatter = new BinaryFormatter();
            return (TData)formatter.Deserialize(stream);
        }


        public string GetFormatCode () {
            return "bin";
        }

    }

}