using System;
using UnityEditor;
using UnityEngine;

namespace ShinyOwl.Common.Structures
{
    [CustomPropertyDrawer(typeof(BoolGrid))]
    public class BoolGridPropertyDrawer : PropertyDrawer
    {
        private float _lineHeight = EditorGUIUtility.singleLineHeight;
        private float _verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

        private const int MaxSize = 10; // Width & Height
        private const float ToggleSize = 20f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float y = position.y;

            // Draw the foldout arrow
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, y, position.width, _lineHeight), property.isExpanded, label, true);
            y += _lineHeight + _verticalSpacing;

            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;

            // Draw the dimension properties
            SerializedProperty widthProperty = property.FindPropertyRelative(BoolGrid.WidthName);
            EditorGUI.PropertyField(EditorGUI.IndentedRect(new Rect(position.x, y, position.width, _lineHeight)), widthProperty);
            widthProperty.intValue = Mathf.Clamp(widthProperty.intValue, 1, MaxSize);
            y += _lineHeight + _verticalSpacing;

            SerializedProperty heightProperty = property.FindPropertyRelative(BoolGrid.HeightName);
            EditorGUI.PropertyField(EditorGUI.IndentedRect(new Rect(position.x, y, position.width, _lineHeight)), heightProperty);
            heightProperty.intValue = Mathf.Clamp(heightProperty.intValue, 1, MaxSize);
            y += _lineHeight + _verticalSpacing;

            EditorGUI.indentLevel--;

            // Draw the shape grid
            SerializedProperty boolsProperty = property.FindPropertyRelative(BoolGrid.BoolsName);
            int width = widthProperty.intValue;
            int height = heightProperty.intValue;
            boolsProperty.arraySize = width * height;

            float xPadding = 2f;
            GUI.Box(new Rect(position.x, y, width * ToggleSize + xPadding * 2f, height * ToggleSize + _verticalSpacing * 2f), GUIContent.none, EditorStyles.helpBox);
            y += _verticalSpacing;

            // Builds rows first so we can keep using our local y value
            for (int i = 0; i < height; i++)
            {
                float x = position.x + xPadding * 2f;

                for (int j = 0; j < width; j++)
                {
                    int index = i * width + j;

                    SerializedProperty boolProperty = boolsProperty.GetArrayElementAtIndex(index);
                    boolProperty.boolValue = EditorGUI.Toggle(new Rect(x, y, ToggleSize, ToggleSize), boolProperty.boolValue);

                    x += ToggleSize;
                }

                y += ToggleSize;
            }

            // End of GUI.Box
            y += _verticalSpacing;

            // Draw a toggle all button
            if (GUI.Button(new Rect(position.x, y, position.width, _lineHeight), "Toggle All"))
            {
                bool toggle = true;

                // If at least one bool is true, we will set them all to false 
                for (int i = 0; i < boolsProperty.arraySize; i++)
                {
                    if (boolsProperty.GetArrayElementAtIndex(i).boolValue)
                    {
                        toggle = false;
                        break;
                    }
                }

                for (int i = 0; i < boolsProperty.arraySize; i++)
                {
                    boolsProperty.GetArrayElementAtIndex(i).boolValue = toggle;
                }
            }

            y += _verticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float height = lineHeight;

            if (property.isExpanded)
            {
                // Width & height properties
                height += lineHeight * 2f + _verticalSpacing * 2f;

                // Shape grid
                SerializedProperty heightProperty = property.FindPropertyRelative(BoolGrid.HeightName);
                height += ToggleSize * heightProperty.intValue + _verticalSpacing * 2f;

                // Toggle all button
                height += lineHeight + _verticalSpacing;
            }

            return height;
        }
    }

    [Serializable]
    public class BoolGrid
    {
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private bool[] _bools;

        public static string WidthName => nameof(_width);
        public static string HeightName => nameof(_height);
        public static string BoolsName => nameof(_bools);
    }
}