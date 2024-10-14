using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaveSystemPackage.Internal {

    internal class Logger : ILogger {

        public LogLevel EnabledLogs { get; set; }
        private readonly StringBuilder m_logCache;
        private readonly File m_logFile;


        internal Logger (LogLevel enabledLogs, Directory logDirectory, int logCacheCapacity = 4096) {
            EnabledLogs = enabledLogs;
            m_logFile = logDirectory.CreateFile("save-system", "log");
            m_logCache = new StringBuilder(logCacheCapacity);
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
            lock (m_logCache) {
                if (m_logCache.Length == 0)
                    return;

                WriteToFile(m_logCache.ToString());
                m_logCache.Clear();
            }
        }


        private void WriteLogs (LogLevel logLevel, object sender, object message) {
            var fullMessage = $"[{logLevel}][{DateTime.Now}] {sender}: {message}";

            lock (m_logCache) {
                if (fullMessage.Length > m_logCache.Capacity) {
                    WriteToFile(fullMessage);
                    return;
                }

                if (m_logCache.Capacity < m_logCache.Length + fullMessage.Length)
                    FlushLogs();
                m_logCache.AppendLine(fullMessage);
            }
        }


        private void WriteToFile (string message) {
            lock (m_logFile) {
                Task.Run(() => {
                    using FileStream fileStream = m_logFile.Open(FileMode.Append);
                    fileStream.Write(Encoding.Default.GetBytes(message));
                });
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string FormattedMessage (object sender, object message) {
            return $"<b>{sender}:</b> {message}";
        }

    }

}