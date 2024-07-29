using System;
using System.Linq;
using System.Reflection;
using SaveSystemPackage.Attributes;

namespace SaveSystemPackage.Serialization {

    internal static class SerializationManager {

        internal static void SerializeGraph (SaveWriter writer, object graph) {
            if (graph is IRuntimeSerializationCallbacks obj)
                obj.OnBeforeRuntimeSerialization();
            Serialize(writer, graph);
        }


        internal static void DeserializeGraph (SaveReader reader, object graph) {
            Deserialize(reader, graph);
            if (graph is IRuntimeSerializationCallbacks obj)
                obj.OnAfterRuntimeDeserialization();
        }


        private static void Serialize (SaveWriter writer, object graph) {
            Type type = graph.GetType();
            FieldInfo[] fields = type
               .GetFields()
               .Where(field => !field.IsDefined(typeof(NonRuntimeSerializedAttribute)))
               .Concat(type
                   .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                   .Where(field => field.IsDefined(typeof(RuntimeSerializedFieldAttribute))))
               .ToArray();
            writer.Write(fields.Length);

            foreach (FieldInfo field in fields) {
                writer.Write(field.Name);
                object value = field.GetValue(graph);

                if (field.FieldType.IsPrimitive) {
                    writer.Write(value);
                }
                else if (value is string str) {
                    writer.Write(str);
                }
                else if (field.FieldType.IsArray) {
                    var array = (Array)value;
                    writer.Write(array.Length);
                    Type elementType = field.FieldType.GetElementType() ?? throw new InvalidOperationException();

                    foreach (object element in array) {
                        if (elementType.IsPrimitive)
                            writer.Write(element);
                        else if (element is string elementStr)
                            writer.Write(elementStr);
                        else if (elementType.IsDefined(typeof(RuntimeSerializableAttribute)))
                            Serialize(writer, element);
                    }
                }
                else if (field.FieldType.IsDefined(typeof(RuntimeSerializableAttribute))) {
                    Serialize(writer, value);
                }
            }

            PropertyInfo[] properties = type
               .GetProperties()
               .Where(property => !property.IsDefined(typeof(NonRuntimeSerializedAttribute)))
               .Concat(type
                   .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                   .Where(property => property.IsDefined(typeof(RuntimeSerializedPropertyAttribute))))
               .ToArray();
            writer.Write(properties.Length);

            foreach (PropertyInfo property in properties) {
                writer.Write(property.Name);
                object value = property.GetValue(graph);

                if (property.PropertyType.IsPrimitive) {
                    writer.Write(value);
                }
                else if (value is string str) {
                    writer.Write(str);
                }
                else if (property.PropertyType.IsArray) {
                    var array = (Array)value;
                    writer.Write(array.Length);
                    Type elementType = property.PropertyType.GetElementType() ?? throw new InvalidOperationException();

                    foreach (object element in array) {
                        if (elementType.IsPrimitive)
                            writer.Write(element);
                        else if (element is string elementStr)
                            writer.Write(elementStr);
                        else if (elementType.IsDefined(typeof(RuntimeSerializableAttribute)))
                            Serialize(writer, element);
                    }
                }
                else if (property.PropertyType.IsDefined(typeof(RuntimeSerializableAttribute))) {
                    Serialize(writer, value);
                }
            }
        }


        private static void Deserialize (SaveReader reader, object graph) {
            Type type = graph.GetType();
            var fieldsCount = reader.Read<int>();

            for (var i = 0; i < fieldsCount; i++) {
                string fieldName = reader.ReadString();
                FieldInfo field = type.GetField(
                    fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (field == null)
                    continue;

                if (field.FieldType.IsPrimitive) {
                    field.SetValue(graph, reader.ReadObject(field.FieldType));
                }
                else if (field.FieldType == typeof(string)) {
                    field.SetValue(graph, reader.ReadString());
                }
                else if (field.FieldType.IsArray) {
                    var count = reader.Read<int>();
                    Type elementType = field.FieldType.GetElementType();
                    var array = Array.CreateInstance(elementType ?? throw new InvalidOperationException(), count);

                    for (var j = 0; j < count; j++) {
                        if (elementType.IsPrimitive) {
                            array.SetValue(reader.ReadObject(elementType), j);
                        }
                        else if (elementType == typeof(string)) {
                            array.SetValue(reader.ReadString(), j);
                        }
                        else if (elementType.IsDefined(typeof(RuntimeSerializableAttribute))) {
                            object element = Activator.CreateInstance(elementType);
                            Deserialize(reader, element);
                            array.SetValue(element, j);
                        }
                    }

                    field.SetValue(graph, array);
                }
                else if (field.FieldType.IsDefined(typeof(RuntimeSerializableAttribute))) {
                    object subGraph = Activator.CreateInstance(field.FieldType);
                    Deserialize(reader, subGraph);
                    field.SetValue(graph, subGraph);
                }
            }

            var propertiesCount = reader.Read<int>();

            for (var i = 0; i < propertiesCount; i++) {
                string propertyName = reader.ReadString();
                PropertyInfo property = type.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (property == null)
                    continue;

                if (property.PropertyType.IsPrimitive) {
                    property.SetValue(graph, reader.ReadObject(property.PropertyType));
                }
                else if (property.PropertyType == typeof(string)) {
                    property.SetValue(graph, reader.ReadString());
                }
                else if (property.PropertyType.IsArray) {
                    var count = reader.Read<int>();
                    Type elementType = property.PropertyType.GetElementType();
                    var array = Array.CreateInstance(elementType ?? throw new InvalidOperationException(), count);

                    for (var j = 0; j < count; j++) {
                        if (elementType.IsPrimitive) {
                            array.SetValue(reader.ReadObject(elementType), j);
                        }
                        else if (elementType == typeof(string)) {
                            array.SetValue(reader.ReadString(), j);
                        }
                        else if (elementType.IsDefined(typeof(RuntimeSerializableAttribute))) {
                            object element = Activator.CreateInstance(elementType);
                            Deserialize(reader, element);
                            array.SetValue(element, j);
                        }
                    }

                    property.SetValue(graph, array);
                }
                else if (property.PropertyType.IsDefined(typeof(RuntimeSerializableAttribute))) {
                    object subGraph = Activator.CreateInstance(property.PropertyType);
                    Deserialize(reader, subGraph);
                    property.SetValue(graph, subGraph);
                }
            }
        }

    }

}