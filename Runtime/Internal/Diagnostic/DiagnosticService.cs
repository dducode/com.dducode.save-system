using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SaveSystem.Internal.Diagnostic {

    internal static class DiagnosticService {

        internal static readonly Dictionary<Type, List<ObjectMetadata>> Dict = new();
        internal static int ObjectsCount => Dict.Count;


        [Conditional("UNITY_EDITOR")]
        internal static void AddObject<TObject> (TObject obj, string caller) {
            Type objectType = obj.GetType();

            if (!Dict.ContainsKey(objectType))
                Dict.Add(objectType, new List<ObjectMetadata>());
            Dict[objectType].Add(new ObjectMetadata(caller, GCHandle.Alloc(obj, GCHandleType.Weak)));
        }


        [Conditional("UNITY_EDITOR")]
        internal static void AddObjects<TObject> (IEnumerable<TObject> objects, string caller) {
            TObject[] array = objects.ToArray();
            Type objectsType = array[0].GetType();

            if (!Dict.ContainsKey(objectsType))
                Dict.Add(objectsType, new List<ObjectMetadata>());

            foreach (TObject obj in array)
                Dict[objectsType].Add(new ObjectMetadata(caller, GCHandle.Alloc(obj, GCHandleType.Weak)));
        }


        [Conditional("UNITY_EDITOR")]
        internal static void ClearNullObjects () {
            Type[] keys = Dict.Keys.ToArray();

            for (var i = 0; i < Dict.Count; i++) {
                Type key = keys[i];
                List<ObjectMetadata> list = Dict[key];
                list.RemoveAll(obj => obj.handle.Target == null);

                if (list.Count == 0) {
                    Dict.Remove(key);
                    i--;
                }
            }
        }

    }

}