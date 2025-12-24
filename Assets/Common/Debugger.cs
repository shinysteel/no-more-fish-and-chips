using UnityEngine;
using UnityEngine.Assertions;

namespace ShinyOwl.Common
{
    public static class Debugger
    {
        public static void Log(object context, object message)                           => Debug.Log(FormatLog(context, message?.ToString()));
        public static void LogWarning(object context, object message)                    => Debug.LogWarning(FormatLog(context, message?.ToString()));
        public static void LogError(object context, object message)                      => Debug.LogError(FormatLog(context, message?.ToString()));

        public static void AssertIsTrue(bool condition, object context, string message)  => Assert.IsTrue(condition, FormatLog(context, message));
        public static void AssertIsFalse(bool condition, object context, string message) => Assert.IsFalse(condition, FormatLog(context, message));
        public static void AssertIsNull(object value, object context, string message)    => Assert.IsNull(value, FormatLog(context, message));
        public static void AssertIsNotNull(object value, object context, string message) => Assert.IsNotNull(value, FormatLog(context, message));

        private static string FormatLog(object context, string message)
        {
            return $"[{context?.GetType().Name}] {message}";
        }
    }
}