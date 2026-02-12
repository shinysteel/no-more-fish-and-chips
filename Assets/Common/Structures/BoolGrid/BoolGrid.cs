using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShinyOwl.Common.Structures
{
    [CustomEditor(typeof(BoolGrid))]
    public class BoolGridEditor : Editor
    {
        private SerializedProperty _columnsProperty;
        private SerializedProperty _rowsProperty;
        private SerializedProperty _boolsProperty;

        private const int MaxSize = 10; // Columns & Rows
        private const float ToggleSize = 20f;

        private void OnEnable()
        {
            _columnsProperty = serializedObject.FindProperty(BoolGrid.ColumnsName);
            _rowsProperty = serializedObject.FindProperty(BoolGrid.RowsName);
            _boolsProperty = serializedObject.FindProperty(BoolGrid.BoolsName);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDimensions();
            EditorGUILayout.Space();
            DrawBoolGrid();
            EditorGUILayout.Space();
            DrawToggleAll();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDimensions()
        {
            EditorGUILayout.LabelField("Dimensions", EditorStyles.boldLabel);

            int oldColumns = _columnsProperty.intValue;
            int oldRows = _rowsProperty.intValue;

            EditorGUILayout.PropertyField(_columnsProperty);
            _columnsProperty.intValue = Mathf.Clamp(_columnsProperty.intValue, 1, MaxSize);

            EditorGUILayout.PropertyField(_rowsProperty);
            _rowsProperty.intValue = Mathf.Clamp(_rowsProperty.intValue, 1, MaxSize);

            ResizeBools(oldColumns, oldRows);
        }

        private void ResizeBools(int oldColumns, int oldRows)
        {
            int newColumns = _columnsProperty.intValue;
            int newRows = _rowsProperty.intValue;
            int newSize = newColumns * newRows;

            if (_boolsProperty.arraySize == newSize)
            {
                return;
            }

            bool[] oldBools = new bool[oldColumns * oldRows];
            for (int y = 0; y < oldRows; y++)
            {
                for (int x = 0; x < oldColumns; x++)
                {
                    int index = y * oldColumns + x;
                    oldBools[index] = _boolsProperty.GetArrayElementAtIndex(index).boolValue;
                }
            }

            _boolsProperty.arraySize = newSize;

            // Don't entirely understand it, but we need to reset all values in the new array
            for (int i = 0; i < _boolsProperty.arraySize; i++)
            {
                _boolsProperty.GetArrayElementAtIndex(i).boolValue = false;
            }

            for (int y = 0; y < Mathf.Min(oldRows, newRows); y++)
            {
                for (int x = 0; x < Mathf.Min(oldColumns, newColumns); x++)
                {
                    _boolsProperty.GetArrayElementAtIndex(y * newColumns + x).boolValue = oldBools[y * oldColumns + x];
                }
            }

            BoolGrid grid = (BoolGrid)target;
            grid.RecalculateBounds();
        }

        private void DrawBoolGrid()
        {
            int columns = _columnsProperty.intValue;
            int rows = _rowsProperty.intValue;

            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical();

            // Builds rows first so we can keep using our local y value. We start at rows - 1
            // so that (0, 0) is at the bottom left of the grid
            for (int y = rows - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;
                    SerializedProperty boolProperty = _boolsProperty.GetArrayElementAtIndex(index);
                    boolProperty.boolValue = EditorGUI.Toggle(GUILayoutUtility.GetRect(ToggleSize, ToggleSize, GUILayout.ExpandWidth(false)), boolProperty.boolValue);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawToggleAll()
        {
            if (GUILayout.Button("Toggle All"))
            {
                bool toggle = true;

                // If at least one bool is true, we will set them all to false 
                for (int i = 0; i < _boolsProperty.arraySize; i++)
                {
                    if (_boolsProperty.GetArrayElementAtIndex(i).boolValue)
                    {
                        toggle = false;
                        break;
                    }
                }

                for (int i = 0; i < _boolsProperty.arraySize; i++)
                {
                    _boolsProperty.GetArrayElementAtIndex(i).boolValue = toggle;
                }
            }
        }
    }

    [CreateAssetMenu(fileName = "BoolGrid", menuName = "Common/Structures/BoolGrid")]
    public class BoolGrid : ScriptableObject, IEnumerable<KeyValuePair<Vector2Int, bool>>
    {
        [SerializeField] private int _columns;
        [SerializeField] private int _rows;
        [SerializeField] private bool[] _bools;

        private Vector2Int _pivot;
        private Vector2Int _arrayOffset;

        public int Columns => _columns;
        public int Rows => _rows;
        public Vector2Int Pivot => _pivot;

        private int _minX;
        private int _minY;
        private int _maxX;
        private int _maxY;

        public int MinX => _minX;
        public int MinY => _minY;
        public int MaxX => _maxX;
        public int MaxY => _maxY;

        public static string ColumnsName => nameof(_columns);
        public static string RowsName => nameof(_rows);
        public static string BoolsName => nameof(_bools);

        // You can retrieve cells relative to the pivot here. [-1, -1] is a valid request
        public bool this[int x, int y]
        {
            get
            {
                int arrayX = x + _arrayOffset.x;
                int arrayY = y + _arrayOffset.y;
                return _bools[arrayY * Columns + arrayX];
            }
        }

        public IEnumerator<KeyValuePair<Vector2Int, bool>> GetEnumerator()
        {
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    // We need to subtract the offset rather than add, since we are converting backwards
                    yield return new KeyValuePair<Vector2Int, bool>(new Vector2Int(x, y) - _arrayOffset, _bools[y * _columns + x]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void OnEnable()
        {
            RecalculateBounds();
        }

        public void RecalculateBounds()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (KeyValuePair<Vector2Int, bool> kvp in this)
            {
                minX = Mathf.Min(minX, kvp.Key.x);
                minY = Mathf.Min(minY, kvp.Key.y);
                maxX = Mathf.Max(maxX, kvp.Key.x);
                maxY = Mathf.Max(maxY, kvp.Key.y);
            }

            _minX = minX;
            _minY = minY;
            _maxX = maxX;
            _maxY = maxY;
        }

        public BoolGrid GetRotated(int rotations)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (KeyValuePair<Vector2Int, bool> kvp in this)
            {
                Vector2Int rotated = RotateCell(kvp.Key, rotations);

                minX = Mathf.Min(minX, rotated.x);
                minY = Mathf.Min(minY, rotated.y);
                maxX = Mathf.Max(maxX, rotated.x);
                maxY = Mathf.Max(maxY, rotated.y);
            }

            BoolGrid grid = CreateInstance<BoolGrid>();
            grid._columns = maxX - minX + 1;
            grid._rows = maxY - minY + 1;
            grid._bools = new bool[grid._columns * grid._rows];

            // An offset allows us to store 'negative' cells
            grid._arrayOffset = new Vector2Int(-minX, -minY);

            foreach (KeyValuePair<Vector2Int, bool> kvp in this)
            {
                // Booleans default to false
                if (!kvp.Value)
                {
                    continue;
                }

                Vector2Int rotated = RotateCell(kvp.Key, rotations);

                int arrayX = rotated.x + grid._arrayOffset.x;
                int arrayY = rotated.y + grid._arrayOffset.y;

                grid._bools[arrayY * grid._columns + arrayX] = true;
            }

            grid._minX = minX;
            grid._minY = minY;
            grid._maxX = maxX;
            grid._maxY = maxY;
            
            return grid;
        }

        private Vector2Int RotateCell(Vector2Int cell, int rotations)
        {
            switch (rotations % 4)
            {
                default:
                case 0:
                    return cell;

                case 1:
                    return new Vector2Int(cell.y, -cell.x);

                case 2:
                    return new Vector2Int(-cell.x, -cell.y);

                case 3:
                    return new Vector2Int(-cell.y, cell.x);
            }
        }
    }
}