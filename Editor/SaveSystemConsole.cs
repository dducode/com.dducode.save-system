using System;
using SaveSystemPackage.Editor.ConsoleTabs;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    public class SaveSystemConsole : EditorWindow {

        private const string WindowName = "Save System Console";
        private const string DrawingModeKey = "drawing_mode";

        private Vector2 m_scrollViewPosition;
        private ConsoleTabsNames m_selectedTab;
        private IConsoleTab m_drawableTab;


        private readonly GUIContent[] m_toolbarsContent = {
            new() {
                text = "Saved Files",
                tooltip = "Check your saved files in system directory"
            },
            new() {
                text = "Objects Tracker",
                tooltip = "Track data handlers at runtime"
            },
        };

        private SavedFilesTab m_savedFilesTab;
        private TrackerTab m_trackerTab;


        [MenuItem("Window/" + WindowName)]
        private static void Init () {
            var console = GetWindow<SaveSystemConsole>();
            console.titleContent = new GUIContent {
                image = EditorIconsTool.GetMainIcon(),
                text = WindowName
            };
            console.Show();
        }


        private void OnEnable () {
            m_selectedTab = (ConsoleTabsNames)EditorPrefs.GetInt(DrawingModeKey, (int)ConsoleTabsNames.SavedFiles);
            m_drawableTab = GetConsoleTab(m_selectedTab);
            SaveSystem.OnUpdateSystem += Repaint;
        }


        private void OnDisable () {
            SaveSystem.OnUpdateSystem -= Repaint;
        }


        private void OnGUI () {
            m_selectedTab = DrawTabBar(m_selectedTab);
            m_drawableTab = GetConsoleTab(m_selectedTab);

            m_scrollViewPosition = EditorGUILayout.BeginScrollView(m_scrollViewPosition);
            m_drawableTab.Draw();
            EditorGUILayout.EndScrollView();
        }


        private ConsoleTabsNames DrawTabBar (ConsoleTabsNames selectedTab) {
            GUIStyle style = EditorStyles.toolbarButton;
            EditorGUI.BeginChangeCheck();

            selectedTab = (ConsoleTabsNames)GUILayout.Toolbar((int)selectedTab, m_toolbarsContent, style);

            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetInt(DrawingModeKey, (int)selectedTab);

            return selectedTab;
        }


        private IConsoleTab GetConsoleTab (ConsoleTabsNames selectedTab) {
            switch (selectedTab) {
                case ConsoleTabsNames.SavedFiles:
                    return m_savedFilesTab ??= new SavedFilesTab();
                case ConsoleTabsNames.HandlersTracker:
                    return m_trackerTab ??= new TrackerTab();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}