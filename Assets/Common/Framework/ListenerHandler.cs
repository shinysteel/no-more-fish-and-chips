using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShinyOwl.Common.Framework
{
    public class ListenerHandler<TListener>
    {
        private List<TListener> _listeners = new();

        public void AddListener(TListener listener)
        {
            if (_listeners.Contains(listener))
            {
                Debugger.LogError(this, "A listener is trying to add itself more than once");
                return;
            }

            _listeners.Add(listener);
        }

        public void RemoveListener(TListener listener)
        {
            // This can happen when a listener has potential to stop listening during its lifetime, and wants to be doubly sure on shutdown
            if (!_listeners.Contains(listener))
            {
                return;
            }

            _listeners.Remove(listener);
        }

        private void ForEachListener(Action<TListener> call)
        {
            // Use a copy of the listeners since the collection was observed to be modified
            // in some use cases, such as UI unsubscribing on unload
            foreach (TListener listener in _listeners.ToList())
            {
                // Listeners would never normally be null, but since we are copying the collection
                // to iterate over it, we need to consider that listeners can get destroyed during this loop
                if (listener == null)
                { 
                    continue;
                }

                call(listener);
            }
        }

        public void Dispatch(Action<TListener> call) => ForEachListener(call);
        public void Dispatch<T1>(Action<TListener, T1> call, T1 arg1) => ForEachListener(listener => call(listener, arg1));
        public void Dispatch<T1, T2>(Action<TListener, T1, T2> call, T1 arg1, T2 arg2) => ForEachListener(listener => call(listener, arg1, arg2));
        public void Dispatch<T1, T2, T3>(Action<TListener, T1, T2, T3> call, T1 arg1, T2 arg2, T3 arg3) => ForEachListener(listener => call(listener, arg1, arg2, arg3));
    }
}