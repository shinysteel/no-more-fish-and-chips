using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace ShinyOwl.Common.Utils
{
    public static partial class Utils
    {
        public static class Collections
        {
            public static void ResizeList<T>(List<T> list, int newLength, Func<T> createElement, Action<T> removeElement, Action<T, int> processElement)
            {
                for (int i = list.Count; i < newLength; i++)
                {
                    list.Add(createElement());
                }

                for (int i = list.Count - 1; i >= newLength; i--)
                {
                    removeElement(list[i]);
                    list.RemoveAt(i);
                }

                for (int i = 0; i < newLength; i++)
                {
                    processElement(list[i], i);
                }
            }

            public static void RemoveDictionaryKeys<T, U>(Dictionary<T, U> dictionary, Func<KeyValuePair<T, U>, bool> predicate)
            {
                List<T> list = ListPool<T>.Get();

                foreach (KeyValuePair<T, U> kvp in dictionary)
                {
                    if (predicate(kvp))
                    {
                        list.Add(kvp.Key);
                    }
                }

                foreach (T key in list)
                {
                    dictionary.Remove(key);
                }

                ListPool<T>.Release(list);
            }
        }
    }
}