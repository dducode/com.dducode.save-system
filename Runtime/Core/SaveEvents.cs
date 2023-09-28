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
        /// This event will be executed when the player will exit the game
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Core will start saving all handlers, but async handlers will be run in the thread pool
        /// and the Core will wait for them on the main thread.
        /// </para>
        /// <para>
        /// If you want to manually save handlers and then quit the game, you can use
        /// <see cref="SaveSystemCore.SaveAndQuit"/> method
        /// </para>
        /// </remarks>
        OnExit = 8,

        /// <summary>
        /// All events will be executed
        /// </summary>
        All = ~0

    }

}