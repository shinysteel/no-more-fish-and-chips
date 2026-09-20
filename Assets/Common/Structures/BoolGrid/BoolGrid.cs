using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShinyOwl.Common.Structures
{
#if UNITY_EDITOR
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
#endif

    [CreateAssetMenu(fileName = "BoolGrid", menuName = "Common/Structures/BoolGrid")]
    public class BoolGrid : ScriptableObject, IEnumerable<KeyValuePair<Vector2Int, bool>>
    {
        [SerializeField] private int _columns;
        [SerializeField] private int _rows;
        [SerializeField] private bool[] _bools;

        public int Columns => _columns;
        public int Rows => _rows;

        private Vector2Int _arrayOffset;

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

        private int CellToIndex(Vector2Int cell)
        {
            int arrayX = cell.x + _arrayOffset.x;
            int arrayY = cell.y + _arrayOffset.y;
            return arrayY * _columns + arrayX;
        }

        // You can retrieve cells relative to the pivot here
        public bool this[Vector2Int cell]
        {
            get
            {
                return _bools[CellToIndex(cell)];
            }
        }

        public bool TryGetBool(Vector2Int cell, out bool value)
        {
            value = false;

            int index = CellToIndex(cell);

            if (index < 0 || index >= _bools.Length)
            {
                return false;
            }

            value = _bools[index];

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

        public IEnumerable<Vector2Int> Keys
        {
            get
            {
                foreach (KeyValuePair<Vector2Int, bool> kvp in this)
                {
                    yield return kvp.Key;
                }
            }
        }

        public IEnumerable<bool> Values
        {
            get
            {
                foreach (KeyValuePair<Vector2Int, bool> kvp in this)
                {
                    yield return kvp.Value;
                }
            }
        }

        // foreach in 'this' is not so readable for what it's doing, so it's preferred to use these methods
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

        // Recalculates bounds and counts. Needs to be public so that the editor script can access it
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

            foreach (KeyValuePair<Vector2Int, bool> kvp in this)
            {
                minGridX = Mathf.Min(minGridX, kvp.Key.x);
                minGridY = Mathf.Min(minGridY, kvp.Key.y);
                maxGridX = Mathf.Max(maxGridX, kvp.Key.x);
                maxGridY = Mathf.Max(maxGridY, kvp.Key.y);

                cellCount++;

                if (!kvp.Value)
                {
                    return;
                }

                minTrueX = Mathf.Min(minTrueX, kvp.Key.x);
                minTrueY = Mathf.Min(minTrueY, kvp.Key.y);
                maxTrueX = Mathf.Max(maxTrueX, kvp.Key.x);
                maxTrueY = Mathf.Max(maxTrueY, kvp.Key.y);

                trueCount++;
            };

            _gridBounds = new RectInt(minGridX, minGridY, maxGridX - minGridX, maxGridY - minGridY);
            _trueBounds = new RectInt(minTrueX, minTrueY, maxTrueX - minTrueX, maxTrueY - minTrueY);

            _cellCount = cellCount;
            _trueCount = trueCount;
        }

        public BoolGrid GetTransformed(Vector2Int pivot, int rotations)
        {
            int minGridX = int.MaxValue;
            int minGridY = int.MaxValue;
            int maxGridX = int.MinValue;
            int maxGridY = int.MinValue;

            foreach (Vector2Int cell in Keys)
            {
                Vector2Int rotated = Utils.Utils.Math.RotateCell(cell - pivot, rotations, true);

                minGridX = Mathf.Min(minGridX, rotated.x);
                minGridY = Mathf.Min(minGridY, rotated.y);
                maxGridX = Mathf.Max(maxGridX, rotated.x);
                maxGridY = Mathf.Max(maxGridY, rotated.y);
            }

            BoolGrid grid = CreateInstance<BoolGrid>();
            grid._columns = maxGridX - minGridX + 1;
            grid._rows = maxGridY - minGridY + 1;
            grid._bools = new bool[grid._columns * grid._rows];

            // An offset allows us to store 'negative' cells
            grid._arrayOffset = new Vector2Int(-minGridX, -minGridY);

            ForEachTrue((Vector2Int cell) =>
            {
                Vector2Int rotated = Utils.Utils.Math.RotateCell(cell - pivot, rotations, true);

                int arrayX = rotated.x + grid._arrayOffset.x;
                int arrayY = rotated.y + grid._arrayOffset.y;

                grid._bools[arrayY * grid._columns + arrayX] = true;
            });

            grid.RecalculateVariables();
            
            return grid;
        }
    }
}