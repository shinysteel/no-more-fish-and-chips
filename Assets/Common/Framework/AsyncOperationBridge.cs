using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace ShinyOwl.Common.Framework
{
    /// <summary>
    /// Combines the behaviour IEnumerators, Tasks, and AsyncOperations to form a bridge
    /// that allows them to be used together with much less effort
    /// </summary>
    public class AsyncOperationBridge : CustomYieldInstruction
    {
        // We wrap the generic AsyncOperation to reuse most of its implementation
        private AsyncOperationBridge<object> _wrappedOp;

        public override bool keepWaiting => _wrappedOp.keepWaiting;

        // It's unlikely to want to unsubscribe from an async operation, but if wanting to do
        // so, its not possible here since we need to transform the input to go into our generic wrapper
        public event Action completed
        {
            add    { _wrappedOp.completed += _ => value?.Invoke(); }
            remove {                                               }
        }

        public AsyncOperationBridge(AsyncOperation operation)
        {
            _wrappedOp = new AsyncOperationBridge<object>(operation, _ => null);
        }

        public AsyncOperationBridge(Task task)
        {
            TaskCompletionSource<object> tcs = new();

            Action<Task> continuation = (Task t) =>
            {
                if (t.IsFaulted)
                {
                    tcs.SetException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    tcs.SetCanceled();
                }
                else if (t.IsCompletedSuccessfully)
                {
                    tcs.SetResult(null);
                }
            };

            task.ContinueWith(continuation, TaskScheduler.FromCurrentSynchronizationContext());

            _wrappedOp = new AsyncOperationBridge<object>(tcs.Task);
        }

        public Task GetTask()
        {
            return _wrappedOp.GetTask();
        }

        public TaskAwaiter GetAwaiter()
        {
            return GetTask().GetAwaiter();
        }
    }

    public class AsyncOperationBridge<T> : CustomYieldInstruction
    {
        private bool _isCompleted;
        private T _result;
        private TaskCompletionSource<T> _tcs = new();

        public T Result => _result;
        public override bool keepWaiting => !_isCompleted;

        public event Action<T> completed;

        public AsyncOperationBridge(AsyncOperation operation, Func<AsyncOperation, T> getResult)
        {
            operation.completed += _ =>
            {
                if (_isCompleted)
                {
                    Debugger.LogError(this, "The same async operation was completed more than once");
                    return;
                }

                _isCompleted = true;

                _result = getResult(operation);
                _tcs.SetResult(_result);

                completed?.Invoke(_result);
            };
        }

        public AsyncOperationBridge(Task<T> task)
        {
            Action<Task<T>> continuation = (Task<T> t) =>
            {
                // Non generic flow will 'set' things a second time,
                // so we need to use TrySet to guard against an exception

                if (t.IsFaulted)
                {
                    _tcs.TrySetException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    _tcs.TrySetCanceled();
                }
                else if (t.IsCompletedSuccessfully)
                {
                    _result = t.Result;

                    _tcs.TrySetResult(_result);
                }

                _isCompleted = true;

                completed?.Invoke(_result);
            };

            task.ContinueWith(continuation, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Task<T> GetTask()
        {
            return _tcs.Task;
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return GetTask().GetAwaiter();
        }
    }
}