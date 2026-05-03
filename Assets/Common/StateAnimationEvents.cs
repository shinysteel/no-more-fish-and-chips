using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

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

    public class StateAnimationEvents : IEnumerable<StateAnimationEvent>
    {
        private string _stateName;
        private bool _canLoop;

        private List<StateAnimationEvent> _events = new();

        private int _loops;
        private int _index;

        public StateAnimationEvents(string stateName, bool canLoop)
        {
            _stateName = stateName;
            _canLoop = canLoop;
        }

        // Implementing IEnumerator allows us to initialise the class with values
        public IEnumerator<StateAnimationEvent> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(StateAnimationEvent animationEvent)
        {
            _events.Add(animationEvent);

            // Ensure the collection is in ascending order
            _events.Sort();
        }

        public void Tick(AnimatorStateInfo info)
        {
            if (!info.IsName(_stateName))
            {
                _loops = 0;
                _index = 0;
                return;
            }

            int newLoops = Mathf.FloorToInt(info.normalizedTime);
            
            if (newLoops > _loops)
            {
                // When starting a new loop, we need to flush any remaining events from the previous loop
                invokeEvents(1f);

                if (_canLoop)
                {
                    _index = 0;
                }
            }

            _loops = newLoops;

            if (!_canLoop && _loops > 0)
            {
                return;
            }

            // When an animation loops, normalisedTime goes beyond 1
            float normalisedTime = info.normalizedTime % 1f;

            // Executes events once their normalised time has exceeded
            invokeEvents(normalisedTime);

            void invokeEvents(float normalisedTime)
            {
                while (_index < _events.Count && _events[_index].NormalisedTime <= normalisedTime)
                {
                    _events[_index].Method?.Invoke();
                    _index++;
                }
            }
        }
    }
}