using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

using Debug = UnityEngine.Debug;

namespace ShinyOwl.Common
{
    public static class Log
    {
        private const string UnityEditor = "UNITY_EDITOR";
        private const string DevelopmentBuild = "DEVELOPMENT_BUILD";

        // Conditionals will include the method as long as one is true
        [Conditional(UnityEditor), Conditional(DevelopmentBuild)]
        public static void Info(object message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Debug.Log(Format(message, filePath, memberName, lineNumber));
        }

        [Conditional(UnityEditor), Conditional(DevelopmentBuild)]
        public static void Warning(object message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Debug.LogWarning(Format(message, filePath, memberName, lineNumber));
        }

        [Conditional(UnityEditor), Conditional(DevelopmentBuild)]
        public static void Error(object message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Debug.LogError(Format(message, filePath, memberName, lineNumber));
        }

        private static string Format(object message, string filePath, string memberName, int lineNumber)
        {
            return $"{GetClassName(filePath)}::{memberName}@{lineNumber}: {message}";
        }

        private static string GetClassName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath.Replace('\\', Path.DirectorySeparatorChar));
        }
    }
}