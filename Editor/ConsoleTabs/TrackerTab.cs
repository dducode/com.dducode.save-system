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

            DrawNumberColumn(headerStyle, entryStyle);
            DrawFilePathColumn(headerStyle, entryStyle);
            DrawCreateFromColumn(headerStyle, entryStyle);
            DrawHandlerTypeColumn(headerStyle, entryStyle);
            DrawObjectsTypeColumn(headerStyle, entryStyle);
            DrawObjectsCountColumn(headerStyle, entryStyle);

            EditorGUILayout.EndHorizontal();
        }


        private void DrawNumberColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Number", headerStyle);

            for (var i = 0; i < DiagnosticService.HandlersData.Count; i++)
                EditorGUILayout.LabelField($"{i + 1}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawFilePathColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Destination Path", headerStyle);

            foreach (HandlerMetadata metadata in DiagnosticService.HandlersData)
                EditorGUILayout.LabelField($"{metadata.destinationPath}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawCreateFromColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Create From", headerStyle);

            foreach (HandlerMetadata metadata in DiagnosticService.HandlersData)
                EditorGUILayout.LabelField($"{metadata.caller}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawHandlerTypeColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Handler Type", headerStyle);

            foreach (HandlerMetadata metadata in DiagnosticService.HandlersData)
                EditorGUILayout.LabelField($"{metadata.handlerType.Name}", entryStyle);
            
            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsTypeColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objects Type", headerStyle);

            foreach (HandlerMetadata metadata in DiagnosticService.HandlersData)
                EditorGUILayout.LabelField($"{metadata.objectsType.Name}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsCountColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objects Count", headerStyle);

            foreach (HandlerMetadata metadata in DiagnosticService.HandlersData)
                EditorGUILayout.LabelField($"{metadata.objectsCount}", entryStyle);

            EditorGUILayout.EndVertical();
        }

    }

}