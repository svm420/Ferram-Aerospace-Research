using System;
using System.Diagnostics;
using UnityEngine;

namespace FerramAerospaceResearch.FARUtils
{
    public static class FARLogger
    {

        private static string tag = "[FAR]";

        public static string Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = "[" + value + "]";
            }
        }

        #region Info
        public static void Info(object message) => UnityEngine.Debug.Log(Tag + " " + message);

        public static void Info(object message, UnityEngine.Object context) => UnityEngine.Debug.Log(Tag + " " + message, context);

        public static void InfoFormat(string format, params object[] args) => UnityEngine.Debug.LogFormat(Tag + " " + format, args);

        public static void InfoFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogFormat(context, Tag + " " + format, args);

        public static void InfoWithCaller(object message) => UnityEngine.Debug.Log(Tag + " " + GetCallerInfo() + " - " + message);

        public static void InfoWithCaller(object message, UnityEngine.Object context) => UnityEngine.Debug.Log(Tag + " " + GetCallerInfo() + " - " + message, context);

        public static void InfoFormatWithCaller(string format, params object[] args) => UnityEngine.Debug.LogFormat(Tag + " " + GetCallerInfo() + " - " + format, args);

        public static void InfoFormatWithCaller(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogFormat(context, Tag + " " + GetCallerInfo() + " - " + format, args);
        #endregion // Info

        #region Debug
        [Conditional("DEBUG")]
        public static void Debug(object message) => UnityEngine.Debug.Log(Tag + " " + message);

        [Conditional("DEBUG")]
        public static void Debug(object message, UnityEngine.Object context) => UnityEngine.Debug.Log(Tag + " " + message, context);

        [Conditional("DEBUG")]
        public static void DebugFormat(string format, params object[] args) => UnityEngine.Debug.LogFormat(Tag + " " + format, args);

        [Conditional("DEBUG")]
        public static void DebugFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogFormat(context, Tag + " " + format, args);

        [Conditional("DEBUG")]
        public static void DebugWithCaller(object message) => UnityEngine.Debug.Log(Tag + " " + GetCallerInfo() + " - " + message);

        [Conditional("DEBUG")]
        public static void DebugWithCaller(object message, UnityEngine.Object context) => UnityEngine.Debug.Log(Tag + " " + GetCallerInfo() + " - " + message, context);

        [Conditional("DEBUG")]
        public static void DebugFormatWithCaller(string format, params object[] args) => UnityEngine.Debug.LogFormat(Tag + " " + GetCallerInfo() + " - " + format, args);

        [Conditional("DEBUG")]
        public static void DebugFormatWithCaller(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogFormat(context, Tag + " " + GetCallerInfo() + " - " + format, args);
        #endregion // Debug

        #region Warning
        public static void Warning(object message) => UnityEngine.Debug.LogWarning(Tag + " " + message);

        public static void Warning(object message, UnityEngine.Object context) => UnityEngine.Debug.LogWarning(Tag + " " + message, context);

        public static void WarningFormat(string format, params object[] args) => UnityEngine.Debug.LogWarningFormat(Tag + " " + format, args);

        public static void WarningFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogWarningFormat(context, Tag + " " + format, args);

        public static void WarningWithCaller(object message) => UnityEngine.Debug.LogWarning(Tag + " " + GetCallerInfo() + " - " + message);

        public static void WarningWithCaller(object message, UnityEngine.Object context) => UnityEngine.Debug.LogWarning(Tag + " " + GetCallerInfo() + " - " + message, context);

        public static void WarningFormatWithCaller(string format, params object[] args) => UnityEngine.Debug.LogWarningFormat(Tag + " " + GetCallerInfo() + " - " + format, args);

        public static void WarningFormatWithCaller(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogWarningFormat(context, Tag + " " + GetCallerInfo() + " - " + format, args);
        #endregion // Warning

        #region Error
        public static void Error(object message) => UnityEngine.Debug.LogError(Tag + " " + message);

        public static void Error(object message, UnityEngine.Object context) => UnityEngine.Debug.LogError(Tag + " " + message, context);

        public static void ErrorFormat(string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(Tag + " " + format, args);

        public static void ErrorFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(context, Tag + " " + format, args);

        public static void ErrorWithCaller(object message) => UnityEngine.Debug.LogError(Tag + " " + GetCallerInfo() + " - " + message);

        public static void ErrorWithCaller(object message, UnityEngine.Object context) => UnityEngine.Debug.LogError(Tag + " " + GetCallerInfo() + " - " + message, context);

        public static void ErrorFormatWithCaller(string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(Tag + " " + GetCallerInfo() + " - " + format, args);

        public static void ErrorFormatWithCaller(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(context, Tag + " " + GetCallerInfo() + " - " + format, args);
        #endregion // Error

        #region Assertion
        public static void Assertion(object message) => UnityEngine.Debug.LogAssertion(Tag + " " + message);

        public static void Assertion(object message, UnityEngine.Object context) => UnityEngine.Debug.LogAssertion(Tag + " " + message, context);

        public static void AssertionFormat(string format, params object[] args) => UnityEngine.Debug.LogAssertionFormat(Tag + " " + format, args);

        public static void AssertionFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogAssertionFormat(context, Tag + " " + format, args);

        public static void AssertionWithCaller(object message) => UnityEngine.Debug.LogAssertion(Tag + " " + GetCallerInfo() + " - " + message);

        public static void AssertionWithCaller(object message, UnityEngine.Object context) => UnityEngine.Debug.LogAssertion(Tag + " " + GetCallerInfo() + " - " + message, context);

        public static void AssertionFormatWithCaller(string format, params object[] args) => UnityEngine.Debug.LogAssertionFormat(Tag + " " + GetCallerInfo() + " - " + format, args);

        public static void AssertionFormatWithCaller(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogAssertionFormat(context, Tag + " " + GetCallerInfo() + " - " + format, args);
        #endregion // Assertion

        #region Exception
        public static void Exception(Exception exception)
        {
            Error("Logged exception:");
            UnityEngine.Debug.LogException(exception);
        }

        public static void Exception(Exception exception, UnityEngine.Object context)
        {
            Error("Logged exception:");
            UnityEngine.Debug.LogException(exception, context);
        }

        public static void ExceptionWithCaller(Exception exception)
        {
            Error(GetCallerInfo() + " - Logged exception:");
            UnityEngine.Debug.LogException(exception);
        }

        public static void ExceptionWithCaller(Exception exception, UnityEngine.Object context)
        {
            Error(GetCallerInfo() + " - Logged exception:");
            UnityEngine.Debug.LogException(exception, context);
        }
        #endregion // Exception

        //http://geekswithblogs.net/BlackRabbitCoder/archive/2013/07/25/c.net-little-wonders-getting-caller-information.aspx
        private static string GetCallerInfo()
        {
            // frame 0 - GetCallerInfo
            // frame 1 - one of the log methods
            // frame 2 - source of the log method caller
            //             StackFrame frame = new StackFrame(2, true);
            //             string method = frame.GetMethod().Name;
            // #if DEBUG
            //             // release mode doesn't have debug symbols
            //             string fileName = frame.GetFileName();
            //             int lineNumber = frame.GetFileLineNumber();

            //             return $"{fileName}({lineNumber}):{method}";
            // #else
            //             return method
            // #endif
            string trace = System.Environment.StackTrace;
            if (String.IsNullOrEmpty(trace))
            {
                return "";
            }
            string[] lines = trace.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string caller = lines[3].Trim();
            return caller.Substring(0, caller.IndexOf("("));
        }

#if NET_NEWER_4_5
        // For reference if KSP .NET version is updated
        // there's a much faster method to obtain caller info in .NET >= 4.5
        public static string MessageWithCallerInfo(object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return fileName + "(" + lineNumber + "):" + memberName + " - " + message;
        }
#endif
    }
}
