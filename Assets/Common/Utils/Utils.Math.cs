using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ShinyOwl.Common.Utils
{
    public enum Axis
    {
        Horizontal,
        Vertical
    }

    public static partial class Utils
    {
        public static class Math
        {
            public static float SquareRootOfTwo = Mathf.Sqrt(2f);

            public static int EuclideanModulo(int dividend, int modulus)
            {
                int remainder = dividend % modulus;

                if (remainder < 0)
                {
                    remainder += modulus;
                }

                return remainder;
            }

            public static Vector2 RotateCell(Vector2 cell, int rotations, bool clockwise)
            {
                rotations = EuclideanModulo(clockwise ? rotations : -rotations, 4);

                return (rotations % 4) switch
                {
                    0 => cell,
                    1 => new Vector2(cell.y, -cell.x),
                    2 => new Vector2(-cell.x, -cell.y),
                    3 => new Vector2(-cell.y, cell.x),

                    _ => cell
                };
            }

            public static Vector2Int RotateCell(Vector2Int cell, int rotations, bool clockwise)
            {
                return Vector2Int.RoundToInt(RotateCell((Vector2)cell, rotations, clockwise));
            }

            public static Vector3 RoundVector3(Vector3 vector3, int precision)
            {
                return new Vector3(
                    (float)System.Math.Round(vector3.x, precision), 
                    (float)System.Math.Round(vector3.y, precision), 
                    (float)System.Math.Round(vector3.z, precision));
            }

            public static Quaternion RoundQuaternion(Quaternion quaternion, int precision)
            {
                return new Quaternion(
                    (float)System.Math.Round(quaternion.x, precision),
                    (float)System.Math.Round(quaternion.y, precision),
                    (float)System.Math.Round(quaternion.z, precision),
                    (float)System.Math.Round(quaternion.w, precision));
            }

            public static int HashLongToInt(long value)
            {
                return (int)(value ^ (value >> 32));
            }

            public static int ManhattanDistance(Vector2Int cellA, Vector2Int cellB)
            {
                return Mathf.Abs(cellA.x - cellB.x) + Mathf.Abs(cellA.y - cellB.y);
            }

            public static bool IsAdjacent(Vector2Int cellA, Vector2Int cellB)
            {
                return ManhattanDistance(cellA, cellB) == 1;
            }

            public static bool HasAdjacency<T>(IReadOnlyDictionary<Vector2Int, T> dictionary, Vector2Int cell, Func<T, bool> condition)
            {
                bool adjacent(Vector2Int cell)
                {
                    return dictionary.TryGetValue(cell, out T value) && (condition?.Invoke(value) ?? true);
                }

                for (int i = -1; i <= 1; i += 2)
                {
                    if (adjacent(new Vector2Int(cell.x + i, cell.y)) || adjacent(new Vector2Int(cell.x, cell.y + i)))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}