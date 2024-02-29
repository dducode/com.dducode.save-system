using System;
using System.Collections.Generic;
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

            DiagnosticService.ClearNullObjects();
            DrawNumberColumn(headerStyle, entryStyle);
            DrawObjectsTypeColumn(headerStyle, entryStyle);
            DrawObjectsCountColumn(headerStyle, entryStyle);

            EditorGUILayout.EndHorizontal();
        }


        private void DrawNumberColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Number", headerStyle);

            for (var i = 0; i < DiagnosticService.ObjectsCount; i++)
                EditorGUILayout.LabelField($"{i + 1}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsTypeColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objects Type", headerStyle);

            foreach (KeyValuePair<Type, List<ObjectMetadata>> metadata in DiagnosticService.Dict)
                EditorGUILayout.LabelField($"{metadata.Key.Name}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsCountColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objects Count", headerStyle);

            foreach (KeyValuePair<Type, List<ObjectMetadata>> metadata in DiagnosticService.Dict)
                EditorGUILayout.LabelField($"{metadata.Value.Count}", entryStyle);

            EditorGUILayout.EndVertical();
        }

    }

}