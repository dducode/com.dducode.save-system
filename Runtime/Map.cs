using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SaveSystemPackage {

    [XmlRoot("map")]
    [Serializable]
    public class Map<Tkey, TValue> : Dictionary<Tkey, TValue>, IXmlSerializable {

        public Map () { }
        public Map (Dictionary<string, string> dictionary) : base((IDictionary<Tkey, TValue>)dictionary) { }


        public virtual XmlSchema GetSchema () {
            return null;
        }


        public virtual void ReadXml (XmlReader reader) {
            if (reader.IsEmptyElement)
                return;

            var keySerializer = new XmlSerializer(typeof(Tkey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement) {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                object key = keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                object value = valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadEndElement();

                Add((Tkey)key, (TValue)value);
                reader.Read();
            }
        }


        public virtual void WriteXml (XmlWriter writer) {
            var keySerializer = new XmlSerializer(typeof(Tkey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            foreach ((Tkey key, TValue value) in this) {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

    }

}