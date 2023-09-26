using System;

namespace SaveSystem.Core {

    /// <summary>
    /// Defines save events that can be managed by external code
    /// </summary>
    /// <remarks> You can combine events with each other </remarks>
    /// <example> EnabledEvents = SaveEvents.AutoSave | SaveEvents.OnFocusChanged </example>
    [Flags]
    public enum SaveEvents {

        /// <summary>
        /// No save events will be invoked
        /// </summary>
        None = 0,

        /// <summary>
        /// Autosave will be executed once during each save period
        /// </summary>
        AutoSave = 1,

        /// <summary>
        /// If the application lose focus, this event will be invoked
        /// </summary>
        OnFocusChanged = 2,

        /// <summary>
        /// This event will be invoked when the application receives a low-memory notification
        /// </summary>
        OnLowMemory = 4,

        /// <summary>
        /// All events will be executed
        /// </summary>
        All = ~0

    }

}