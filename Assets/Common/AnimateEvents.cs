using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

namespace ShinyOwl.Common
{
    public class AnimateEvent
    {
        private float _normalisedTime;
        private Action _method;

        public float NormalisedTime => _normalisedTime;
        public Action Method => _method;

        public AnimateEvent(float normalisedTime, Action method)
        {
            SetNormalisedTime(normalisedTime);
            _method = method;
        }

        public void SetNormalisedTime(float time)
        {
            _normalisedTime = time;
        }
    }

    public class AnimateEvents : IEnumerable<AnimateEvent>
    {
        private List<AnimateEvent> _events = new();

        public IEnumerator<AnimateEvent> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(AnimateEvent animateEvent)
        {
            _events.Add(animateEvent);
        }

        public AnimateEvents()
        {
            // Only accept valid values for normalised time
            for (int i = 0; i < _events.Count; i++)
            {
                _events[i].SetNormalisedTime(Mathf.Clamp(_events[i].NormalisedTime, 0f, 1f));
            }

            // Ensure the collection is in ascending order
            _events.Sort((a, b) =>
            {
                return a.NormalisedTime.CompareTo(b.NormalisedTime);
            });
        }

        public async Task PlayAsync(Animator animator, int layerIndex, string stateName)
        {
            // Wait until the desired state is active
            while (!animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName))
            {
                await Task.Yield();
            }

            int index = 0;

            // Executes events once their normalised time has exceeded
            while (true)
            {
                AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layerIndex);
                float time = info.normalizedTime;

                while (index < _events.Count && (_events[index].NormalisedTime <= time || !info.IsName(stateName)))
                {
                    _events[index].Method?.Invoke();
                    index++;
                }

                if (index < _events.Count)
                {
                    await Task.Yield();
                }
                else
                {
                    break;
                }
            }
        }
    }
}