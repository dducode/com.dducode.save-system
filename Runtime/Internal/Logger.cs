using System.Runtime.CompilerServices;
using UnityEngine;

namespace SaveSystem.Internal {

    internal static class Logger {

        private static readonly string MessageHeader = $"<b>{nameof(SaveSystem)}:</b>";


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Log<TMessage> (TMessage message) {
            Debug.Log(FormattedMessage(message));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning<TMessage> (TMessage message) {
            Debug.LogWarning(FormattedMessage(message));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError<TMessage> (TMessage message) {
            Debug.LogError(FormattedMessage(message));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FormattedMessage<TMessage> (TMessage message) {
            return $"{MessageHeader} {message}";
        }

    }

}