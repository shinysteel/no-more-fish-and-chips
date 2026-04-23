using FishFlingers.Entities;
using NUnit.Framework.Internal;
using PrimeTween;
using UnityEngine;

namespace ShinyOwl.Common.Extensions
{
    public static class TweenExtensions
    {
        // Uses Quaternion.Slerp, which PrimeTween for some reason doesn't
        public static Tween Rotation(Transform target, Quaternion endValue, float duration, Ease ease)
        {
            Quaternion startValue = target.rotation;
            return Tween.Custom(0f, 1f, duration, (float time) => target.rotation = Quaternion.Slerp(startValue, endValue, time), ease);
        }
    }
}