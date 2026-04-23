using System;
using UnityEngine;

namespace ShinyOwl.Common.Utils
{
    public enum Axis
    {
        Horizontal,
        Vertical
    }

    // Make sure this enum is clockwise, as methods like FlipDirection assume it as so
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public static partial class Utils
    {
        public static class Math
        {
            public static Vector2Int DirectionToVector2Int(Direction direction)
            {
                return direction switch
                {
                    Direction.Up => Vector2Int.up,
                    Direction.Right => Vector2Int.right,
                    Direction.Down => Vector2Int.down,
                    Direction.Left => Vector2Int.left,
                    _ => Vector2Int.zero
                };
            }

            public static Vector3 DirectionToVector3(Direction direction)
            {
                return direction switch
                {
                    Direction.Up => Vector3.forward,
                    Direction.Right => Vector3.right,
                    Direction.Down => Vector3.back,
                    Direction.Left => Vector3.left,
                    _ => Vector3.zero
                };
            }

            public static Direction FlipDirection(Direction direction)
            {
                return (Direction)EuclideanModulo((int)direction + 2, 4);
            }

            public static Direction PerpendicularDirection(Direction direction, bool clockwise)
            {
                return (Direction)EuclideanModulo((int)direction + (clockwise ? 1 : -1), 4);
            }

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
        }
    }
}