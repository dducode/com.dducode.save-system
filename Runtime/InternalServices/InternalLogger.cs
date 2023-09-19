﻿using System.Runtime.CompilerServices;
using UnityEngine;

namespace SaveSystem.InternalServices {

    internal static class InternalLogger {

        private static readonly string MessageHeader = $"<b>{nameof(SaveSystem)}:</b>";


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Log (string message) {
            Debug.Log($"{MessageHeader} {message}");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning (string message) {
            Debug.LogWarning($"{MessageHeader} {message}");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError (string message) {
            Debug.LogError($"{MessageHeader} {message}");
        }

    }

}