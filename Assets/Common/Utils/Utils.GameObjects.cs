using System;
using UnityEngine;

namespace ShinyOwl.Common.Utils
{
    public static partial class Utils
    {
        public static class GameObjects
        {
            public static void TraverseHierarchy(GameObject obj, Action<GameObject> action)
            {
                action(obj);

                foreach (Transform child in obj.transform)
                {
                    TraverseHierarchy(child.gameObject, action);
                }
            }
        }
    }
}