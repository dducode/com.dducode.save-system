using System.Collections.Generic;
using System.Diagnostics;

namespace SaveSystem.Internal.Diagnostic {

    internal static class DiagnosticService {

        internal static readonly List<DynamicObjectGroupMetadata> GroupsData = new();
        internal static int HandlersCount => GroupsData.Count;


        [Conditional("UNITY_EDITOR")]
        internal static void AddMetadata (DynamicObjectGroupMetadata metadata) {
            GroupsData.Add(metadata);
        }


        [Conditional("UNITY_EDITOR")]
        internal static void UpdateObjectsCount (int index, int count) {
            if (index >= GroupsData.Count)
                return;

            GroupsData[index].objectsCount = count;
        }


        [Conditional("UNITY_EDITOR")]
        internal static void ClearNullGroups () {
            for (var i = 0; i < GroupsData.Count; i++)
                if (GroupsData[i].handle.Target == null)
                    GroupsData.RemoveAt(i--);
        }

    }

}