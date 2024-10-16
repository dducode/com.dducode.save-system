﻿using System;

namespace SaveSystemPackage {

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
        /// If the application lose focus, this event will be invoked
        /// </summary>
        OnFocusLost = 1,

        /// <summary>
        /// This event will be invoked when the application receives a low-memory notification
        /// </summary>
        OnLowMemory = 2,

        /// <summary>
        /// Periodic save will be executed once during each save period
        /// </summary>
        PeriodicSave = 4,

        /// <summary>
        /// All events will be executed
        /// </summary>
        All = ~0

    }

}