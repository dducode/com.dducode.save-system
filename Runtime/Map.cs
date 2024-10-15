using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace SaveSystemPackage {

    [XmlRoot("map")]
    [Serializable]
    public class Map<Tkey, TValue> : Dictionary<Tkey, TValue>, IXmlSerializable, ISerializationCallbackReceiver {

        [SerializeField]
        private SerializableKeyValuePair<Tkey, TValue>[] map;

        public Map () { }
        public Map (Dictionary<Tkey, TValue> dictionary) : base(dictionary) { }
        protected Map (SerializationInfo info, StreamingContext context) : base(info, context) { }


        public virtual XmlSchema GetSchema () {
            return null;
        }


        public void ReadXml (XmlReader reader) {
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


        public void WriteXml (XmlWriter writer) {
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


        public void OnBeforeSerialize () {
            map = this
               .Select(item => (SerializableKeyValuePair<Tkey, TValue>)item)
               .ToArray();
        }


        public void OnAfterDeserialize () {
            foreach ((Tkey key, TValue value) in map)
                Add(key, value);
        }

    }

}