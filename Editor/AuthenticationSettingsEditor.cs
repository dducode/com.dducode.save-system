using System;
using SaveSystem.Security;
using UnityEditor;

namespace SaveSystem.Editor {

    [CustomEditor(typeof(AuthenticationSettings))]
    public class AuthenticationSettingsEditor : UnityEditor.Editor {

        private SerializedProperty m_hashAlgorithmProperty;
        private SerializedProperty m_globalAuthHashKeyProperty;
        private SerializedProperty m_profileAuthHashKeyProperty;


        private void OnEnable () {
            m_hashAlgorithmProperty = serializedObject.FindProperty("hashAlgorithm");
            m_globalAuthHashKeyProperty = serializedObject.FindProperty("globalAuthHashKey");
            m_profileAuthHashKeyProperty = serializedObject.FindProperty("profileAuthHashKey");
        }


        public override void OnInspectorGUI () {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_hashAlgorithmProperty);
            DrawingUtilities.DrawKeyProperty(
                m_globalAuthHashKeyProperty, "Generate Global Auth Key", Guid.NewGuid().ToString
            );
            DrawingUtilities.DrawKeyProperty(
                m_profileAuthHashKeyProperty, "Generate Profile Auth Key", Guid.NewGuid().ToString
            );

            serializedObject.ApplyModifiedProperties();
        }

    }

}