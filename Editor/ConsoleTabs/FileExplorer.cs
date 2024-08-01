using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using File = SaveSystemPackage.Internal.File;

namespace SaveSystemPackage.Editor.ConsoleTabs {

    internal class FileExplorer : IConsoleTab {

        private const string TypeKey = "saved_files_tab";
        private const string ShowInternalKey = TypeKey + "show_internal";

        private readonly Dictionary<string, bool> m_directories = new();
        private bool m_showInternal = EditorPrefs.GetBool(ShowInternalKey, false);

        private string m_selectedEntry;
        private int m_indentLevel;


        public void Draw () {
            DrawDataSizeLabel();
            DrawProperties();
            EditorGUILayout.Space(15);
            DrawFileSystemEntries(Storage.Root);

            if (Storage.HasAnyData())
                DrawDeleteDataButton();
        }


        private void DrawDataSizeLabel () {
            string message = Storage.GetFormattedDataSize();
            EditorGUILayout.HelpBox($"Total data size: {message}", MessageType.Info);
        }


        private void DrawProperties () {
            EditorGUI.BeginChangeCheck();
            m_showInternal = EditorGUILayout.ToggleLeft("Show Internal Folder", m_showInternal);

            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool(ShowInternalKey, m_showInternal);
        }


        private void DrawFileSystemEntries (Internal.Directory directory) {
            foreach (Internal.Directory nested in directory.EnumerateDirectories())
                DrawDirectory(nested);

            foreach (File file in directory.EnumerateFiles())
                DrawFile(file);
        }


        private void DrawDirectory (Internal.Directory directory) {
            if (!m_showInternal && directory == Storage.InternalDirectory)
                return;
            if (!directory.Exists)
                return;

            m_directories.TryAdd(directory.Name, false);

            using (new EditorGUILayout.HorizontalScope()) {
                Event ev = Event.current;

                if (ev.clickCount > 1 && EditorGUILayout.GetControlRect().Contains(ev.mousePosition))
                    EditorUtility.RevealInFinder(directory.Path);

                if (directory.IsEmpty) {
                    EditorGUILayout.LabelField(new GUIContent {
                        text = directory.Name,
                        image = EditorGUIUtility.IconContent("Folder Icon").image
                    });
                }
                else {
                    m_directories[directory.Name] = EditorGUILayout.Foldout(
                        m_directories[directory.Name],
                        new GUIContent {
                            text = directory.Name,
                            image = EditorGUIUtility.IconContent("Folder Icon").image
                        }
                    );
                }
            }

            EditorGUI.indentLevel++;
            if (m_directories[directory.Name])
                DrawFileSystemEntries(directory);
            EditorGUI.indentLevel--;
        }


        private void DrawFile (File file) {
            if (!file.Exists)
                return;

            using var scope = new EditorGUILayout.HorizontalScope();

            Event ev = Event.current;

            if (ev.clickCount > 1 && EditorGUILayout.GetControlRect().Contains(ev.mousePosition))
                EditorUtility.RevealInFinder(file.Path);

            EditorGUILayout.LabelField(new GUIContent {
                text = file.FullName,
                image = EditorGUIUtility.IconContent("DefaultAsset Icon").image
            });

            string fileSize = Storage.GetFormattedDataSize(file.DataSize);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField(fileSize);
            EditorGUI.indentLevel = indent;
        }


        private void DrawDeleteDataButton () {
            var allowDeleteData = false;

            const string dialogName = "Delete All Data";

            if (GUILayout.Button(dialogName, GUILayout.ExpandWidth(false))) {
                allowDeleteData = EditorUtility.DisplayDialog(
                    dialogName,
                    "Are you sure to delete all saved data?",
                    "Yes", "No"
                );
            }

            if (allowDeleteData) {
                Storage.DeleteAllData();
                EditorUtility.DisplayDialog(dialogName, "Data deleted successfully", "Ok");
            }
        }

    }

}