using Newtonsoft.Json;
using System;
using UnityEngine;

namespace ShinyOwl.Common.Utils
{
    public static partial class Utils
    {
        public static class Serialisation
        { }
    }

    public class SimpleVector2Int
    {
        [JsonProperty] public int X { get; private set; }
        [JsonProperty] public int Y { get; private set; }

        public SimpleVector2Int() : this(Vector2Int.zero)
        { }

        public SimpleVector2Int(Vector2Int vector2Int)
        {
            X = vector2Int.x;
            Y = vector2Int.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(X, Y);
        }
    }

    public class SimpleVector3
    {
        [JsonProperty] public float X { get; private set; }
        [JsonProperty] public float Y { get; private set; }
        [JsonProperty] public float Z { get; private set; }

        public SimpleVector3() : this(Vector3.zero)
        { }

        public SimpleVector3(Vector3 vector3)
        {
            X = vector3.x;
            Y = vector3.y;
            Z = vector3.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }

    public class SimpleQuaternion
    {
        [JsonProperty] public float X { get; private set; }
        [JsonProperty] public float Y { get; private set; }
        [JsonProperty] public float Z { get; private set; }
        [JsonProperty] public float W { get; private set; }

        public SimpleQuaternion() : this(Quaternion.identity)
        { }

        public SimpleQuaternion(Quaternion quaternion)
        {
            X = quaternion.x;
            Y = quaternion.y;
            Z = quaternion.z;
            W = quaternion.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(X, Y, Z, W);
        }
    }
}