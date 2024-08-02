using SaveSystemPackage.ComponentsRecording;
using SaveSystemPackage.Internal.Cryptography;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomEditor(typeof(ComponentRecorder), true)]
    public class ComponentRecorderEditor : UnityEditor.Editor {

        private SerializedProperty m_idProperty;


        private void OnEnable () {
            m_idProperty = serializedObject.FindProperty("id");
        }


        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            if (string.IsNullOrEmpty(m_idProperty.stringValue) || !IsUniqueId((ComponentRecorder)target)) {
                m_idProperty.stringValue = CryptoUtilities.GenerateKey(8);
                serializedObject.ApplyModifiedProperties();
            }
        }


        private bool IsUniqueId (ComponentRecorder @this) {
            ComponentRecorder[] recorders = FindObjectsByType<ComponentRecorder>(FindObjectsSortMode.None);

            foreach (ComponentRecorder recorder in recorders)
                if (recorder != @this && string.Equals(recorder.Id, @this.Id))
                    return false;

            return true;
        }

    }

}