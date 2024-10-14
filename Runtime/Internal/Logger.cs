using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaveSystemPackage.Internal {

    internal class Logger : ILogger, IDisposable, IAsyncDisposable {

        public LogLevel EnabledLogs { get; set; }
        private readonly FileStream m_logStream;
        private readonly StringBuilder m_logCache;


        internal Logger (LogLevel enabledLogs, Directory logDirectory, int logCacheCapacity = 4096) {
            EnabledLogs = enabledLogs;
            m_logStream = logDirectory.CreateFile("save-system", "log").Open();
            m_logCache = new StringBuilder(logCacheCapacity);
        }


        ~Logger () {
            Dispose();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log (object sender, object message, Object context = null) {
            if (!EnabledLogs.HasFlag(LogLevel.Debug))
                return;

            Debug.Log(FormattedMessage(sender, message), context);
            WriteLogs(LogLevel.Debug, sender, message);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning (object sender, object message, Object context = null) {
            if (!EnabledLogs.HasFlag(LogLevel.Warning))
                return;

            Debug.LogWarning(FormattedMessage(sender, message), context);
            WriteLogs(LogLevel.Warning, sender, message);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError (object sender, object message, Object context = null) {
            if (!EnabledLogs.HasFlag(LogLevel.Error))
                return;

            Debug.LogError(FormattedMessage(sender, message), context);
            WriteLogs(LogLevel.Error, sender, message);
            FlushLogs();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException (object sender, Exception exception) {
            Debug.LogException(exception);
            WriteLogs(LogLevel.Error, sender, exception.Message);
            WriteLogs(LogLevel.Error, null, exception.StackTrace);
            FlushLogs();
        }


        public void FlushLogs () {
            if (m_logCache.Length == 0)
                return;

            m_logStream.Write(Encoding.Default.GetBytes(m_logCache.ToString()));
            m_logCache.Clear();
        }


        public void Dispose () {
            m_logStream?.Dispose();
        }


        public async ValueTask DisposeAsync () {
            if (m_logStream != null)
                await m_logStream.DisposeAsync();
        }


        private void WriteLogs (LogLevel logLevel, object sender, object message) {
            var fullMessage = $"[{logLevel}][{DateTime.Now}] {sender}: {message}";
            if (m_logCache.Capacity < m_logCache.Length + fullMessage.Length)
                FlushLogs();
            m_logCache.AppendLine(fullMessage);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string FormattedMessage (object sender, object message) {
            return $"<b>{sender}:</b> {message}";
        }

    }

}