using UnityEngine;
using NoMoreFishAndChips.Audio;
using NoMoreFishAndChips.Cameras;
using PurrNet;
using ShinyOwl.Common;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace ShinyOwl.Common.Utils
{
    public static partial class Utils
    {
        public static class Physics
        {
            public static Vector3 GetProjectilePosition(Vector3 startPosition, Vector3 endPosition, float gravity, float launchAngle, float normalisedTime)
            {
                Vector3 direction = endPosition - startPosition;
                Vector3 directionXZ = new Vector3(direction.x, 0f, direction.z);
                float distance = directionXZ.magnitude;
                float radians = launchAngle * Mathf.Deg2Rad;
                float cos = Mathf.Cos(radians);
                float sin = Mathf.Sin(radians);
                float height = direction.y;
                float speed = (gravity * distance * distance) / (2f * cos * cos * (distance * Mathf.Tan(radians) - height));
                speed = Mathf.Sqrt(speed);
                Vector3 velocity = directionXZ.normalized * speed * cos + Vector3.up * speed * sin;
                float time = distance / (speed * cos);
                float t = time * normalisedTime;
                return startPosition + velocity * t + Vector3.down * (0.5f * gravity * t * t);
            }

            public static int CapsuleCastNonAlloc(CapsuleCollider collider, Vector3 offset, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask)
            {
                Vector3 center = collider.transform.position + collider.transform.TransformVector(collider.center) + offset;
                float radius = collider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.z);
                float height = Mathf.Max(collider.height * collider.transform.lossyScale.y, radius * 2f);
                float pointOffset = height * 0.5f - radius;

                Vector3 point1 = center - Vector3.up * pointOffset;
                Vector3 point2 = center + Vector3.up * pointOffset;

                return UnityEngine.Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, results, maxDistance, layerMask);
            }
        }
    }
}