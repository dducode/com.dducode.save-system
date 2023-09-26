using System.IO;
using SaveSystem.Internal;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.ConsoleTabs {

    internal class SavedFilesTab : IConsoleTab {

        public void Draw () {
            DrawDataSizeLabel();
            DrawFileSystemEntries(Application.persistentDataPath);

            if (Storage.HasAnyData())
                DrawDeleteDataButton();
        }


        private void DrawDataSizeLabel () {
            string message = Storage.GetFormattedDataSize();
            EditorGUILayout.HelpBox($"Total data size: {message}", MessageType.Info);
        }


        private void DrawFileSystemEntries (string path) {
            string openEntryLabel = SystemInfo.operatingSystem.StartsWith("Windows")
                ? "Show in explorer"
                : "Reveal in finder";

            foreach (string entryPath in Directory.GetFileSystemEntries(path)) {
                EditorGUILayout.BeginHorizontal();

                if (Directory.Exists(entryPath)) {
                    DrawFolderEntry(openEntryLabel, entryPath);
                    continue;
                }

                DrawFileEntry(openEntryLabel, entryPath);

                EditorGUILayout.EndHorizontal();
            }
        }


        private void DrawFolderEntry (string openEntryLabel, string entryPath) {
            EditorGUILayout.LabelField(new GUIContent {
                text = Path.GetFileName(entryPath),
                image = EditorGUIUtility.IconContent("Folder Icon").image
            });

            if (GUILayout.Button(openEntryLabel, GUILayout.ExpandWidth(false)))
                EditorUtility.RevealInFinder(entryPath);

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            DrawFileSystemEntries(entryPath);
            EditorGUI.indentLevel--;
        }


        private void DrawFileEntry (string openEntryLabel, string entryPath) {
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