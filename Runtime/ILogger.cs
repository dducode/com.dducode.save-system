using System;
using Object = UnityEngine.Object;

namespace SaveSystemPackage {

    public interface ILogger {

        public LogLevel EnabledLogs { get; set; }
        public void Log (object sender, object message, Object context = null);
        public void LogWarning (object sender, object message, Object context = null);
        public void LogError (object sender, object message, Object context = null);
        public void LogException (object sender, Exception exception);
        public void FlushLogs ();

    }

}