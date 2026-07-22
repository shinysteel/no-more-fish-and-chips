using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace ShinyOwl.Common.Utils
{
    public static partial class Utils
    {
        public static class Tasks
        {
            public static Task WaitForFixedUpdateAsync()
            {
                TaskCompletionSource<bool> tcs = new();

                CoroutineRunner.instance.StartCoroutine(coroutine());

                IEnumerator coroutine()
                {
                    yield return new WaitForFixedUpdate();
                    tcs.SetResult(true);
                }

                return tcs.Task;
            }
        }
    }
}