using System;

namespace SaveSystem {

    [Flags]
    public enum LogLevel {

        /// <summary>
        /// All logs will be disabled
        /// </summary>
        None = 0,

        /// <summary>
        /// Debug logs will be enabled
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Warning logs will be enabled
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Error logs will be enabled
        /// </summary>
        Error = 4,

        /// <summary>
        /// All logs will be enabled
        /// </summary>
        All = ~0

    }

}