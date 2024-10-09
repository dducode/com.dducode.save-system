using System;
using System.Collections.Generic;
using System.IO;
using SaveSystemPackage.Exceptions;
using SaveSystemPackage.Internal.Extensions;

namespace SaveSystemPackage.Serialization {

    public class BinarySerializer : ISerializer {

        private static readonly Dictionary<Type, bool> s_unmanagedTypes = new();


        public byte[] Serialize<TData> (TData data) where TData : ISaveData {
            using var stream = new MemoryStream();
            using var writer = new SaveWriter(stream);

            Type type = typeof(TData);
            bool isUnmanaged = IsUnmanagedType(type);

            if (isUnmanaged) {
                writer.Write(data);
            }
            else if (data is IBinarySerializable serializable) {
                serializable.WriteBinary(writer);
            }
            else {
                throw new BinarySerializationException(
                    $"Cannot serialize \"{type}\" type because it's managed type and it doesn't implement \"{nameof(IBinarySerializable)}\""
                );
            }

            return stream.ToArray();
        }


        public TData Deserialize<TData> (byte[] data) where TData : ISaveData {
            using var stream = new MemoryStream(data);
            using var reader = new SaveReader(stream);

            Type type = typeof(TData);
            bool isUnmanaged = IsUnmanagedType(type);

            if (isUnmanaged) {
                return (TData)reader.ReadObject(type);
            }
            else {
                var deserializedObject = Activator.CreateInstance<TData>();

                if (deserializedObject is IBinarySerializable serializable) {
                    serializable.ReadBinary(reader);
                    return (TData)serializable;
                }
                else {
                    throw new BinarySerializationException(
                        $"Cannot deserialize \"{type}\" type because it's managed type and it doesn't implement \"{nameof(IBinarySerializable)}\""
                    );
                }
            }
        }


        public string GetFormatCode () {
            return "bin";
        }


        private bool IsUnmanagedType (Type type) {
            if (!s_unmanagedTypes.ContainsKey(type))
                s_unmanagedTypes.Add(type, type.IsUnmanaged());
            return s_unmanagedTypes[type];
        }

    }

}