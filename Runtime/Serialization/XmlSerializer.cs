﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Serialization {

    public class XmlSerializer : ISerializer {

        public byte[] Serialize<TData> ([NotNull] TData data) where TData : ISaveData {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.IsEmpty)
                return Array.Empty<byte>();

            using var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TData));
                serializer.Serialize(writer, data);
            }

            return stream.ToArray();
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            if (data == null || data.Length == 0)
                return default;

            using var reader = new StreamReader(new MemoryStream(data), Encoding.UTF8);
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TData));
            return (TData)serializer.Deserialize(reader);
        }


        public string GetFormatCode () {
            return CodeFormats.XML;
        }

    }

}