using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SaveSystemPackage.Internal.Diagnostic;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor.ConsoleTabs {

    internal class TrackerTab : IConsoleTab {

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

            foreach (KeyValuePair<Type, List<GCHandle>> metadata in DiagnosticService.Dict) {
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
            EditorGUILayout.LabelField("Objects Count", headerStyle);

            foreach (KeyValuePair<Type, List<GCHandle>> metadata in DiagnosticService.Dict)
                EditorGUILayout.LabelField($"{metadata.Value.Count}", entryStyle);

            EditorGUILayout.EndVertical();
        }

    }

}