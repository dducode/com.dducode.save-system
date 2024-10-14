using System.Runtime.CompilerServices;
using UnityEngine;

namespace SaveSystemPackage.Internal {

    internal class Logger {

        internal LogLevel EnabledLogs { get; set; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Log (object sender, object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Debug))
                Debug.Log(FormattedMessage(sender, message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LogWarning (object sender, object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Warning))
                Debug.LogWarning(FormattedMessage(sender, message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LogError (object sender, object message, Object context = null) {
            if (EnabledLogs.HasFlag(LogLevel.Error))
                Debug.LogError(FormattedMessage(sender, message), context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal string FormattedMessage (object sender, object message) {
            return $"<b>{sender}:</b> {message}";
        }

    }

}