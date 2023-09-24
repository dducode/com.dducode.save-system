namespace SaveSystem {

    /// <summary>
    /// <a href="https://dducode.github.io/save-system-docs/manual/async-modes.html">See manual</a>
    /// </summary>
    public enum AsyncMode {

        /// <summary>
        /// Sets the data processing at the player loop (main thread)
        /// </summary>
        OnPlayerLoop,

        /// <summary>
        /// Sets the data processing at the thread pool
        /// </summary>
        OnThreadPool,

    }

}