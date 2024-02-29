using System.Runtime.CompilerServices;
using UnityEngine;

namespace SaveSystem.Internal {

    internal static class Logger {

        private static readonly string MessageHeader = $"<b>{nameof(SaveSystem)}:</b>";

        internal static LogLevel EnabledLogs { get; set; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Log (object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Debug))
                Debug.Log(FormattedMessage(message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning (object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Warning))
                Debug.LogWarning(FormattedMessage(message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError (object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Error))
                Debug.LogError(FormattedMessage(message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FormattedMessage (object message) {
            return $"{MessageHeader} {message}";
        }

    }

}