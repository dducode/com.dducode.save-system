using SaveSystem.Internal.Diagnostic;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class TrackerTab : IConsoleTab {

        public void Draw () {
            if (!EditorApplication.isPlaying) {
                EditorGUILayout.HelpBox("You can tracking handlers only at runtime", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUIStyle headerStyle = EditorStyles.wordWrappedLabel;
            GUIStyle entryStyle = EditorStyles.wordWrappedMiniLabel;

            DiagnosticService.ClearNullGroups();
            DrawNumberColumn(headerStyle, entryStyle);
            DrawCreateFromColumn(headerStyle, entryStyle);
            DrawObjectsTypeColumn(headerStyle, entryStyle);
            DrawObjectsCountColumn(headerStyle, entryStyle);

            EditorGUILayout.EndHorizontal();
        }


        private void DrawNumberColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Number", headerStyle);

            for (var i = 0; i < DiagnosticService.HandlersCount; i++)
                EditorGUILayout.LabelField($"{i + 1}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawCreateFromColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Create From", headerStyle);

            foreach (DynamicObjectGroupMetadata metadata in DiagnosticService.GroupsData)
                EditorGUILayout.LabelField($"{metadata.caller}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsTypeColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objects Type", headerStyle);

            foreach (DynamicObjectGroupMetadata metadata in DiagnosticService.GroupsData)
                EditorGUILayout.LabelField($"{metadata.objectsType.Name}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsCountColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objects Count", headerStyle);

            foreach (DynamicObjectGroupMetadata metadata in DiagnosticService.GroupsData)
                EditorGUILayout.LabelField($"{metadata.objectsCount}", entryStyle);

            EditorGUILayout.EndVertical();
        }

    }

}