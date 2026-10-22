
using System;
using UnityEngine;

namespace ShinyOwl.Common
{
    public class StateAnimationEvent : IComparable<StateAnimationEvent>
    {
        private float _normalisedTime;
        private Action _method;

        public float NormalisedTime => _normalisedTime;
        public Action Method => _method;

        public StateAnimationEvent(float normalisedTime, Action method)
        {
            SetNormalisedTime(normalisedTime);
            _method = method;
        }

        public void SetNormalisedTime(float time)
        {
            _normalisedTime = Mathf.Clamp(time, 0f, 1f);
        }

        public int CompareTo(StateAnimationEvent other)
        {
            return _normalisedTime.CompareTo(other._normalisedTime);
        }
    }
}