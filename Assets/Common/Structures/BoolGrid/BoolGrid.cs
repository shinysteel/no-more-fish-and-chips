using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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
            grid.RecalculateVariables();
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

        public int Columns => _columns;
        public int Rows => _rows;

        private Vector2Int _arrayOffset;

        private Vector2Int _pivot;
        public Vector2Int Pivot => _pivot;

        private RectInt _gridBounds;
        private RectInt _trueBounds;

        public RectInt GridBounds => _gridBounds;
        public RectInt TrueBounds => _trueBounds;

        private int _cellCount;
        private int _trueCount;

        public int CellCount => _cellCount;
        public int TrueCount => _trueCount;

        public static string ColumnsName => nameof(_columns);
        public static string RowsName => nameof(_rows);
        public static string BoolsName => nameof(_bools);

        // You can retrieve cells relative to the pivot here. [-1, -1] is a valid request
        public bool this[Vector2Int cell]
        {
            get
            {
                TryGetBool(cell, out bool value);
                return value;
            }
        }

        public bool TryGetBool(Vector2Int cell, out bool value)
        {
            value = false;

            int arrayX = cell.x + _arrayOffset.x;
            if (arrayX < 0 || arrayX >= _columns)
            {
                return false;
            }

            int arrayY = cell.y + _arrayOffset.y;
            if (arrayY < 0 || arrayY >= _rows)
            {
                return false;
            }

            value = _bools[arrayY * Columns + arrayX];
            return true;
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

        // foreach in 'this' is not so readable for what it's doing, so it's preferred to use these methods
        public void ForEachCell(Action<Vector2Int, bool> action)
        {
            foreach (KeyValuePair<Vector2Int, bool> kvp in this)
            {
                action(kvp.Key, kvp.Value);
            }
        }

        public void ForEachTrue(Action<Vector2Int> action)
        {
            foreach (KeyValuePair<Vector2Int, bool> kvp in this)
            {
                if (kvp.Value)
                {
                    action(kvp.Key);
                }
            }
        }

        private void OnEnable()
        {
            RecalculateVariables();
        }

        // Recalculates bounds and counts
        public void RecalculateVariables()
        {
            int minGridX = int.MaxValue;
            int minGridY = int.MaxValue;
            int maxGridX = int.MinValue;
            int maxGridY = int.MinValue;

            int minTrueX = int.MaxValue;
            int minTrueY = int.MaxValue;
            int maxTrueX = int.MinValue;
            int maxTrueY = int.MinValue;

            int cellCount = 0;
            int trueCount = 0;

            ForEachCell((Vector2Int cell, bool value) =>
            {
                minGridX = Mathf.Min(minGridX, cell.x);
                minGridY = Mathf.Min(minGridY, cell.y);
                maxGridX = Mathf.Max(maxGridX, cell.x);
                maxGridY = Mathf.Max(maxGridY, cell.y);

                cellCount++;

                if (!value)
                {
                    return;
                }

                minTrueX = Mathf.Min(minTrueX, cell.x);
                minTrueY = Mathf.Min(minTrueY, cell.y);
                maxTrueX = Mathf.Max(maxTrueX, cell.x);
                maxTrueY = Mathf.Max(maxTrueY, cell.y);

                trueCount++;
            });

            _gridBounds = new RectInt(minGridX, minGridY, maxGridX - minGridX, maxGridY - minGridY);
            _trueBounds = new RectInt(minTrueX, minTrueY, maxTrueX - minTrueX, maxTrueY - minTrueY);

            _cellCount = cellCount;
            _trueCount = trueCount;
        }

        public BoolGrid GetRotated(int rotations)
        {
            int minGridX = int.MaxValue;
            int minGridY = int.MaxValue;
            int maxGridX = int.MinValue;
            int maxGridY = int.MinValue;

            ForEachCell((Vector2Int cell, bool value) =>
            {
                Vector2Int rotated = RotateCell(cell, rotations);

                minGridX = Mathf.Min(minGridX, rotated.x);
                minGridY = Mathf.Min(minGridY, rotated.y);
                maxGridX = Mathf.Max(maxGridX, rotated.x);
                maxGridY = Mathf.Max(maxGridY, rotated.y);
            });

            BoolGrid grid = CreateInstance<BoolGrid>();
            grid._columns = maxGridX - minGridX + 1;
            grid._rows = maxGridY - minGridY + 1;
            grid._bools = new bool[grid._columns * grid._rows];

            // An offset allows us to store 'negative' cells
            grid._arrayOffset = new Vector2Int(-minGridX, -minGridY);

            ForEachTrue((Vector2Int cell) =>
            {
                Vector2Int rotated = RotateCell(cell, rotations);

                int arrayX = rotated.x + grid._arrayOffset.x;
                int arrayY = rotated.y + grid._arrayOffset.y;

                grid._bools[arrayY * grid._columns + arrayX] = true;
            });

            grid.RecalculateVariables();
            
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