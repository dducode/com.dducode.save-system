using System;

namespace SaveSystem.Internal {

    internal static class StringExtensions {

        internal static bool IsBase64String (this string base64) {
            var buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int _);
        }

    }

}