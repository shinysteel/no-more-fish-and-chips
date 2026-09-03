using NoMoreFishAndChips.Entities;
using NUnit.Framework.Internal;
using PrimeTween;
using UnityEngine;

namespace ShinyOwl.Common
{
    public static class PrimeTweenFix
    {
        // Uses Quaternion.Slerp, which PrimeTween for some reason doesn't
        public static Tween Rotation(Transform target, Quaternion endValue, float duration, Ease ease)
        {
            Quaternion startValue = target.rotation;
            return Tween.Custom(startValue: 0f, endValue: 1f, duration: duration, onValueChange: (float value) => target.rotation = Quaternion.Slerp(startValue, endValue, value), ease: ease);
        }

        public static Tween RigidbodyMoveRotation(Rigidbody target, Quaternion endValue, float duration, Ease ease)
        {
            Quaternion startValue = target.rotation;
            return Tween.Custom(settings: new TweenSettings<float>(startValue: 0f, endValue: 1f, duration: duration, ease: ease, updateType: UpdateType.FixedUpdate), onValueChange: (float value) => target.MoveRotation(Quaternion.Slerp(startValue, endValue, value)));
        }
    }
}