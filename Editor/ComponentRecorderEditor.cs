using SaveSystemPackage.ComponentsRecording;
using SaveSystemPackage.Internal.Cryptography;
using UnityEditor;

namespace SaveSystemPackage.Editor {

    [CustomEditor(typeof(ComponentRecorder), true)]
    public class ComponentRecorderEditor : UnityEditor.Editor {

        private SerializedProperty m_idProperty;


        private void OnEnable () {
            m_idProperty = serializedObject.FindProperty("id");
        }


        public override void OnInspectorGUI () {
            base.OnInspectorGUI();

            if (string.IsNullOrEmpty(m_idProperty.stringValue)) {
                m_idProperty.stringValue = CryptoUtilities.GenerateKey(8);
                serializedObject.ApplyModifiedProperties();
            }
        }

    }

}