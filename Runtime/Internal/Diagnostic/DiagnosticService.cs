using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SaveSystem.Internal.Diagnostic {

    internal static class DiagnosticService {

        internal static readonly Dictionary<Type, List<GCHandle>> Dict = new();
        internal static int ObjectsCount => Dict.Count;


        [Conditional("UNITY_EDITOR")]
        internal static void AddObject<TObject> (TObject obj) {
            Type objectType = obj.GetType();

            if (!Dict.ContainsKey(objectType))
                Dict.Add(objectType, new List<GCHandle>());
            Dict[objectType].Add(GCHandle.Alloc(obj, GCHandleType.Weak));
        }


        [Conditional("UNITY_EDITOR")]
        internal static void AddObjects<TObject> (IEnumerable<TObject> objects) {
            TObject[] array = objects.ToArray();
            if (array.Length == 0)
                return;

            Type objectsType = array.First().GetType();

            if (!Dict.ContainsKey(objectsType))
                Dict.Add(objectsType, new List<GCHandle>());

            foreach (TObject obj in array)
                Dict[objectsType].Add(GCHandle.Alloc(obj, GCHandleType.Weak));
        }


        [Conditional("UNITY_EDITOR")]
        internal static void ClearNullObjects () {
            Type[] keys = Dict.Keys.ToArray();

            for (var i = 0; i < Dict.Count; i++) {
                Type key = keys[i];
                List<GCHandle> list = Dict[key];
                list.RemoveAll(handle => handle.Target == null);

                if (list.Count == 0) {
                    Dict.Remove(key);
                    i--;
                }
            }
        }

    }

}