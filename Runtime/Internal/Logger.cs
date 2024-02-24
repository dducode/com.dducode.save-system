using System.Runtime.CompilerServices;
using UnityEngine;

namespace SaveSystem.Internal {

    internal static class Logger {

        private static readonly string MessageHeader = $"<b>{nameof(SaveSystem)}:</b>";

        internal static LogLevel EnabledLogs { get; set; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Log<TMessage> (TMessage message) {
            if (EnabledLogs.HasFlag(LogLevel.Debug))
                Debug.Log(FormattedMessage(message));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning<TMessage> (TMessage message) {
            if (EnabledLogs.HasFlag(LogLevel.Warning))
                Debug.LogWarning(FormattedMessage(message));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError<TMessage> (TMessage message) {
            if (EnabledLogs.HasFlag(LogLevel.Error))
                Debug.LogError(FormattedMessage(message));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FormattedMessage<TMessage> (TMessage message) {
            return $"{MessageHeader} {message}";
        }

    }

}