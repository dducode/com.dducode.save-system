using System.Runtime.CompilerServices;
using UnityEngine;

namespace SaveSystem.Internal {

    internal static class Logger {

        internal static LogLevel EnabledLogs { get; set; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Log (object sender, object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Debug))
                Debug.Log(FormattedMessage(sender, message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning (object sender, object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Warning))
                Debug.LogWarning(FormattedMessage(sender, message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError (object sender, object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Error))
                Debug.LogError(FormattedMessage(sender, message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FormattedMessage (object sender, object message) {
            return $"<b>{sender}:</b> {message}";
        }

    }

}