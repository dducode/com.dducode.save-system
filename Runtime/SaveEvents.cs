using System;

namespace SaveSystem {

    /// <summary>
    /// Defines save events that can be managed by external code
    /// </summary>
    /// <remarks> You can combine events with each other </remarks>
    [Flags]
    public enum SaveEvents {

        /// <summary>
        /// No save events will be not invoked
        /// </summary>
        None = 0,

        /// <summary>
        /// Autosave will be executed once during each save period
        /// </summary>
        AutoSave = 1,

        /// <summary>
        /// If the application lose focus, this event will be invoked
        /// </summary>
        OnFocusLost = 2,

        /// <summary>
        /// This event will be invoked when the application receives a low-memory notification
        /// </summary>
        OnLowMemory = 4,

        /// <summary>
        /// This event will be executed when the player will exit the game
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is not supported in the Editor
        /// </para>
        /// </remarks>
        OnExit = 8,

        /// <summary>
        /// All events will be executed
        /// </summary>
        All = ~0

    }

}