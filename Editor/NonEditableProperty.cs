using SaveSystemPackage.Attributes;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    [CustomPropertyDrawer(typeof(NonEditableAttribute))]
    public class NonEditableProperty : PropertyDrawer {

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 0;
        }


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(property);
            GUI.enabled = true;
        }

    }

}