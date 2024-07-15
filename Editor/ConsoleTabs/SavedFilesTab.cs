using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class SavedFilesTab : IConsoleTab {

        private const string TypeKey = "saved_files_tab";
        private const string ShowInternalKey = TypeKey + "show_internal";

        private bool m_showInternal = EditorPrefs.GetBool(ShowInternalKey, false);


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
            string openEntryLabel = SystemInfo.operatingSystem.StartsWith("Windows")
                ? "Show in explorer"
                : "Reveal in finder";

            foreach (string entryPath in Directory.GetFileSystemEntries(path)) {
                if (File.GetAttributes(entryPath).HasFlag(FileAttributes.Directory))
                    DrawFolderEntry(openEntryLabel, entryPath);
                else
                    DrawFileEntry(openEntryLabel, entryPath);
            }
        }


        private void DrawFolderEntry (string openEntryLabel, string entryPath) {
            if (!m_showInternal && entryPath == SaveSystemCore.InternalFolder)
                return;

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(new GUIContent {
                    text = Path.GetFileName(entryPath),
                    image = EditorGUIUtility.IconContent("Folder Icon").image
                });

                if (GUILayout.Button(openEntryLabel, GUILayout.ExpandWidth(false)))
                    EditorUtility.RevealInFinder(entryPath);
            }

            EditorGUI.indentLevel++;
            DrawFileSystemEntries(entryPath);
            EditorGUI.indentLevel--;
        }


        private void DrawFileEntry (string openEntryLabel, string entryPath) {
            using var scope = new EditorGUILayout.HorizontalScope();

            EditorGUILayout.LabelField(new GUIContent {
                text = Path.GetFileName(entryPath),
                image = EditorGUIUtility.IconContent("DefaultAsset Icon").image
            });

            string fileSize = Storage.GetFormattedDataSize(new FileInfo(entryPath).Length);
            EditorGUILayout.LabelField(fileSize);

            if (GUILayout.Button(openEntryLabel, GUILayout.ExpandWidth(false)))
                EditorUtility.RevealInFinder(entryPath);
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