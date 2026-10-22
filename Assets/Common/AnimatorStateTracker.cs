using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

namespace ShinyOwl.Common
{
    public class AnimatorStateTracker
    {
        private string _stateName;
        private bool _canLoop;

        private List<StateAnimationEvent> _events = new();

        private int _loops;
        private int _index;

        private bool _isSkipped;

        public event Action<float> OnNormalisedTimeChanged;

        public AnimatorStateTracker(string stateName, bool canLoop)
        {
            _stateName = stateName;
            _canLoop = canLoop;
        }

        public void AddEvent(StateAnimationEvent animationEvent)
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
                _isSkipped = false;
                return;
            }

            if (_isSkipped)
            {
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

            OnNormalisedTimeChanged?.Invoke(normalisedTime);

            void invokeEvents(float normalisedTime)
            {
                while (_index < _events.Count && _events[_index].NormalisedTime <= normalisedTime)
                {
                    _events[_index].Method?.Invoke();
                    _index++;
                }
            }
        }

        // Skips invoking any events until the next time we are in this state
        public void Skip()
        {
            _isSkipped = true;
        }
    }
}