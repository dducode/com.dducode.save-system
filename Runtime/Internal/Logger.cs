using System.Runtime.CompilerServices;
using UnityEngine;

namespace SaveSystem.Internal {

    internal static class Logger {

        private static readonly string MessageHeader = $"<b>{nameof(SaveSystem)}:</b>";


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Log<TMessage> (TMessage message) {
            Debug.Log($"{MessageHeader} {message}");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning<TMessage> (TMessage message) {
            Debug.LogWarning($"{MessageHeader} {message}");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError<TMessage> (TMessage message) {
            Debug.LogError($"{MessageHeader} {message}");
        }


        internal static string FormattedMessage<TMessage> (TMessage message) {
            return $"{MessageHeader} {message}";
        }

    }

}