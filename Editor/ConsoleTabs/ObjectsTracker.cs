using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaveSystemPackage.Internal.Diagnostic;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor.ConsoleTabs {

    internal class ObjectsTracker : IConsoleTab {

        private readonly List<int> m_totalCount = new();
        private readonly List<int> m_totalSize = new();
        private readonly List<int> m_clearSize = new();


        public void Draw () {
            if (!EditorApplication.isPlaying) {
                EditorGUILayout.HelpBox("You can tracking registered objects only at runtime", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUIStyle headerStyle = EditorStyles.wordWrappedLabel;
            GUIStyle entryStyle = EditorStyles.wordWrappedMiniLabel;

            DiagnosticService.ClearNullObjects();
            DrawNumberColumn(headerStyle, entryStyle);
            DrawObjectsTypeColumn(headerStyle, entryStyle);
            DrawObjectsCountColumn(headerStyle, entryStyle);
            DrawObjectsTotalSizeColumn(headerStyle, entryStyle);
            DrawObjectsClearSizeColumn(headerStyle, entryStyle);

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

            foreach (KeyValuePair<Type, List<ObjectData>> metadata in DiagnosticService.Objects) {
                Type type = metadata.Key;
                string typeName;

                if (!type.IsGenericType) {
                    typeName = type.Name;
                }
                else {
                    Type[] genericArgs = type.GetGenericArguments();
                    var stringBuilder = new StringBuilder();

                    for (var i = 0; i < genericArgs.Length; i++) {
                        Type genericType = genericArgs[i];
                        stringBuilder.Append(genericType.Name);
                        if (i < genericArgs.Length - 1)
                            stringBuilder.Append(", ");
                    }

                    typeName = $"{type.Name}<{stringBuilder}>";
                }

                EditorGUILayout.LabelField(typeName, entryStyle);
            }

            EditorGUILayout.EndVertical();
        }


        private void DrawObjectsCountColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();

            foreach (KeyValuePair<Type, List<ObjectData>> metadata in DiagnosticService.Objects)
                m_totalCount.Add(metadata.Value.Count);

            EditorGUILayout.LabelField($"Objects Count ({m_totalCount.Sum():N0})", headerStyle);

            foreach (int count in m_totalCount)
                EditorGUILayout.LabelField($"{count:N0}", entryStyle);

            EditorGUILayout.EndVertical();
            m_totalCount.Clear();
        }


        private void DrawObjectsTotalSizeColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();

            foreach (KeyValuePair<Type, List<ObjectData>> metadata in DiagnosticService.Objects) {
                var dataSize = 0;
                foreach (ObjectData objectData in metadata.Value)
                    dataSize += objectData.totalDataSize;
                m_totalSize.Add(dataSize);
            }

            EditorGUILayout.LabelField($"Total Size ({Storage.GetFormattedDataSize(m_totalSize.Sum())})", headerStyle);

            foreach (int dataSize in m_totalSize)
                EditorGUILayout.LabelField(Storage.GetFormattedDataSize(dataSize), entryStyle);

            EditorGUILayout.EndVertical();
            m_totalSize.Clear();
        }


        private void DrawObjectsClearSizeColumn (GUIStyle headerStyle, GUIStyle entryStyle) {
            EditorGUILayout.BeginVertical();

            foreach (KeyValuePair<Type, List<ObjectData>> metadata in DiagnosticService.Objects) {
                var dataSize = 0;
                foreach (ObjectData objectData in metadata.Value)
                    dataSize += objectData.clearDataSize;
                m_clearSize.Add(dataSize);
            }

            EditorGUILayout.LabelField($"Clear Size ({Storage.GetFormattedDataSize(m_clearSize.Sum())})", headerStyle);

            foreach (int dataSize in m_clearSize)
                EditorGUILayout.LabelField(Storage.GetFormattedDataSize(dataSize), entryStyle);

            EditorGUILayout.EndVertical();
            m_clearSize.Clear();
        }

    }

}