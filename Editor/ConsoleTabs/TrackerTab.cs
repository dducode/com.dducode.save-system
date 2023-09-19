﻿using SaveSystem.Handlers;
using SaveSystem.InternalServices.Diagnostic;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class TrackerTab : IConsoleTab {

        public void Draw () {
            if (!EditorApplication.isPlaying) {
                EditorGUILayout.HelpBox("You can tracking handlers at runtime only", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUIStyle headerStyle = EditorStyles.wordWrappedLabel;
            GUIStyle entryStyle = EditorStyles.wordWrappedMiniLabel;

            DrawNumberColumn(headerStyle, entryStyle);
            DrawFilePathColumn(headerStyle, entryStyle);
            DrawCreateFromColumn(headerStyle, entryStyle);
            DrawObjectsTypeColumn(headerStyle, entryStyle);
            DrawObjectsCountColumn(headerStyle, entryStyle);

            EditorGUILayout.EndHorizontal();
        }


        private void DrawNumberColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Number", headerStyle);

            for (var i = 0; i < HandlersProvider.HandlersData.Count; i++)
                EditorGUILayout.LabelField($"{i + 1}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawFilePathColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("File Path", headerStyle);

            foreach (HandlerMetadata metadata in HandlersProvider.HandlersData)
                EditorGUILayout.LabelField($"{metadata.filePath}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawCreateFromColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Create From", headerStyle);

            foreach (HandlerMetadata metadata in HandlersProvider.HandlersData)
                EditorGUILayout.LabelField($"{metadata.caller}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsTypeColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objects Type", headerStyle);

            foreach (HandlerMetadata metadata in HandlersProvider.HandlersData)
                EditorGUILayout.LabelField($"{metadata.objectsType}", entryStyle);

            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsCountColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objects Count", headerStyle);

            foreach (HandlerMetadata metadata in HandlersProvider.HandlersData)
                EditorGUILayout.LabelField($"{metadata.objectsCount}", entryStyle);

            EditorGUILayout.EndVertical();
        }

    }

}