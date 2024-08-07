using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SaveSystemPackage.Serialization;

namespace SaveSystemPackage.Internal.Diagnostic {

    internal static class DiagnosticService {

        internal static readonly Dictionary<Type, List<ObjectData>> Objects = new();
        internal static int ObjectsCount => Objects.Count;


        [Conditional("UNITY_EDITOR")]
        internal static void AddObject<TObject> (string key, TObject obj) {
            Type objectType = obj.GetType();

            if (!Objects.ContainsKey(objectType))
                Objects.Add(objectType, new List<ObjectData>());

            Objects[objectType].Add(CreateObjectData(key, obj));
        }


        [Conditional("UNITY_EDITOR")]
        internal static void AddObjects<TObject> (string[] keys, TObject[] objects) {
            if (keys.Length != objects.Length)
                throw new InvalidOperationException("Keys count must match to objects count");
            if (keys.Length == 0)
                return;

            Type objectsType = objects.First().GetType();

            if (!Objects.ContainsKey(objectsType))
                Objects.Add(objectsType, new List<ObjectData>());

            for (var i = 0; i < objects.Length; i++)
                Objects[objectsType].Add(CreateObjectData(keys[i], objects[i]));
        }


        [Conditional("UNITY_EDITOR")]
        internal static void ClearNullObjects () {
            Type[] keys = Objects.Keys.ToArray();

            for (var i = 0; i < Objects.Count; i++) {
                Type key = keys[i];
                List<ObjectData> list = Objects[key];
                list.RemoveAll(data => !data.reference.TryGetTarget(out object _));

                if (list.Count == 0) {
                    Objects.Remove(key);
                    i--;
                }
            }
        }


        [Conditional("UNITY_EDITOR")]
        internal static void Clear () {
            Objects.Clear();
        }


        private static ObjectData CreateObjectData<TObject> (string key, TObject obj) {
            using var stream = new MemoryStream();

            using (var writer = new SaveWriter(stream)) {
                if (obj is IRuntimeSerializable serializable)
                    serializable.Serialize(writer);
                else
                    SerializationManager.SerializeGraph(writer, obj);
            }

            int clearDataSize = stream.ToArray().Length;
            int totalDataSize = clearDataSize + Encoding.UTF8.GetBytes(key).Length;
            return new ObjectData(new WeakReference<object>(obj), totalDataSize, clearDataSize);
        }

    }

}