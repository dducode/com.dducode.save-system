using System;
using SaveSystem.Editor.ConsoleTabs;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor {

    public class SaveSystemConsole : EditorWindow {

        private const string WindowName = "Save System Console";

        private Vector2 m_scrollViewPosition;
        private DrawingMode m_drawingMode;
        private IConsoleTab m_currentTab;

        private readonly GUIContent[] m_toolbarsContent = {
            new() {
                text = "Saved Files",
                tooltip = "Check your saved files in system directory"
            },
            new() {
                text = "Handlers Tracker",
                tooltip = "Track data handlers at runtime"
            },
            new() {
                text = "Settings",
                tooltip = "Configure system settings"
            }
        };


        [MenuItem("Window/" + WindowName)]
        private static void Init () {
            var console = GetWindow<SaveSystemConsole>();
            console.titleContent = new GUIContent {
                image = EditorIconsTool.GetCheckPointsManagerIcon(),
                text = WindowName
            };
            console.Show();
        }


        private void OnEnable () {
            m_currentTab = new SavedFilesTab();
        }


        private void OnGUI () {
            m_currentTab = DrawConsoleTabs(m_currentTab);

            m_scrollViewPosition = EditorGUILayout.BeginScrollView(m_scrollViewPosition);
            m_currentTab.Draw();
            EditorGUILayout.EndScrollView();
        }


        private IConsoleTab DrawConsoleTabs (IConsoleTab currentTab) {
            GUIStyle style = EditorStyles.toolbarButton;
            EditorGUI.BeginChangeCheck();

            m_drawingMode = (DrawingMode)GUILayout.Toolbar((int)m_drawingMode, m_toolbarsContent, style);

            if (EditorGUI.EndChangeCheck()) {
                switch (m_drawingMode) {
                    case DrawingMode.SavedFiles:
                        currentTab = new SavedFilesTab();
                        break;
                    case DrawingMode.HandlersTracker:
                        currentTab = new TrackerTab();
                        break;
                    case DrawingMode.Settings:
                        currentTab = new SettingsTab();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return currentTab;
        }

    }

}