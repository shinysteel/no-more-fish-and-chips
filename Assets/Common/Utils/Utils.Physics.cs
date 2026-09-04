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
            public static int CapsuleCastNonAlloc(CapsuleCollider collider, Vector3 positionOffset, Quaternion rotationOffset, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask)
            {
                Vector3 center = collider.transform.TransformPoint(collider.center + positionOffset);

                Quaternion rotation = collider.transform.rotation * rotationOffset;

                Vector3 scale = collider.transform.lossyScale;

                Vector3 axis = Vector3.zero;
                float radiusScale = 0f;
                float heightScale = 0f;

                switch (collider.direction)
                {
                    case 0:
                        axis = rotation * Vector3.right;
                        radiusScale = Mathf.Max(scale.y, scale.z);
                        heightScale = scale.x;
                        break;

                    case 1:
                        axis = rotation * Vector3.up;
                        radiusScale = Mathf.Max(scale.x, scale.z);
                        heightScale = scale.y;
                        break;

                    case 2:
                        axis = rotation * Vector3.forward;
                        radiusScale = Mathf.Max(scale.x, scale.y);
                        heightScale = scale.z;
                        break;
                }

                float radius = collider.radius * radiusScale;

                float height = Mathf.Max(collider.height * heightScale, radius * 2f);

                float pointOffset = height * 0.5f - radius;

                Vector3 point1 = center - axis * pointOffset;
                Vector3 point2 = center + axis * pointOffset;

                return UnityEngine.Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, results, maxDistance, layerMask);
            }
        }
    }
}