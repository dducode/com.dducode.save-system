using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class SavedFilesTab : IConsoleTab {

        private const string TypeKey = "saved_files_tab";
        private const string ShowInternalKey = TypeKey + "show_internal";

        private readonly Dictionary<string, bool> m_folders = new();
        private bool m_showInternal = EditorPrefs.GetBool(ShowInternalKey, false);

        private string m_selectedEntry;
        private int m_indentLevel;


        public void Draw () {
            DrawDataSizeLabel();
            DrawProperties();
            EditorGUILayout.Space(15);
            DrawFileSystemEntries(Storage.StorageDataPath);

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


        private void DrawFileSystemEntries (string path) {
            foreach (string entryPath in Directory.GetFileSystemEntries(path)) {
                if (File.GetAttributes(entryPath).HasFlag(FileAttributes.Directory))
                    DrawFolderEntry(entryPath);
                else
                    DrawFileEntry(entryPath);
            }
        }


        private void DrawFolderEntry (string entryPath) {
            if (!m_showInternal && entryPath == SaveSystemCore.InternalFolder)
                return;

            m_folders.TryAdd(entryPath, false);

            using (new EditorGUILayout.HorizontalScope()) {
                Event ev = Event.current;

                if (ev.clickCount > 1 && EditorGUILayout.GetControlRect().Contains(ev.mousePosition))
                    EditorUtility.RevealInFinder(entryPath);

                m_folders[entryPath] = EditorGUILayout.Foldout(m_folders[entryPath], new GUIContent {
                    text = Path.GetFileName(entryPath),
                    image = EditorGUIUtility.IconContent("Folder Icon").image
                });
            }

            EditorGUI.indentLevel++;
            if (m_folders[entryPath])
                DrawFileSystemEntries(entryPath);
            EditorGUI.indentLevel--;
        }


        private void DrawFileEntry (string entryPath) {
            using var scope = new EditorGUILayout.HorizontalScope();

            Event ev = Event.current;

            if (ev.clickCount > 1 && EditorGUILayout.GetControlRect().Contains(ev.mousePosition))
                EditorUtility.RevealInFinder(entryPath);

            EditorGUILayout.LabelField(new GUIContent {
                text = Path.GetFileName(entryPath),
                image = EditorGUIUtility.IconContent("DefaultAsset Icon").image
            });

            string fileSize = Storage.GetFormattedDataSize(new FileInfo(entryPath).Length);
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