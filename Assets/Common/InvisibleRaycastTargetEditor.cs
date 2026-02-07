using UnityEditor;
using UnityEngine;

namespace ShinyOwl.Common
{
    [CustomEditor(typeof(InvisibleRaycastTarget))]
    public class InvisibleRaycastTargetEditor : Editor
    {
        private SerializedProperty _raycastTargetProperty;
        private SerializedProperty _raycastPaddingProperty;

        private const string RaycastTargetName = "m_RaycastTarget";
        private const string RaycastPaddingName = "m_RaycastPadding";

        private void OnEnable()
        {
            _raycastTargetProperty = serializedObject.FindProperty(RaycastTargetName);
            _raycastPaddingProperty = serializedObject.FindProperty(RaycastPaddingName);
        }

        public override void OnInspectorGUI()
        {
            // Since the public properties .material and .color are not relevant to what InvisibleRaycastTarget
            // is trying to do, we are going to omit them here

            serializedObject.Update();

            EditorGUILayout.PropertyField(_raycastTargetProperty);
            EditorGUILayout.PropertyField(_raycastPaddingProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}