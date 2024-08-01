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
                text = "File Explorer",
                tooltip = "Check your saved files in save system directory"
            },
            new() {
                text = "Objects Tracker",
                tooltip = "Track registered objects at runtime"
            },
        };

        private FileExplorer m_fileExplorer;
        private ObjectsTracker m_objectsTracker;


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
            m_selectedTab = (ConsoleTabsNames)EditorPrefs.GetInt(DrawingModeKey, (int)ConsoleTabsNames.FileExplorer);
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
                case ConsoleTabsNames.FileExplorer:
                    return m_fileExplorer ??= new FileExplorer();
                case ConsoleTabsNames.ObjectsTracker:
                    return m_objectsTracker ??= new ObjectsTracker();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}