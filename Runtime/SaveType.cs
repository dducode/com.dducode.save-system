namespace SaveSystemPackage {

    /// <summary>
    /// Contains information about the type of saving
    /// </summary>
    public enum SaveType {

        /// <summary>
        /// This is sent when the player uses quick-save
        /// </summary>
        QuickSave,

        /// <summary>
        /// This is sent when the player reaches a checkpoint
        /// </summary>
        SaveAtCheckpoint,

        /// <summary>
        /// This is sent when the system detects any changes in the data
        /// </summary>
        AutoSave,

        /// <summary>
        /// This is sent when the system periodically starts saving in its inner loop
        /// </summary>
        PeriodicSave,

        /// <summary>
        /// This is sent when the application loses focus
        /// </summary>
        OnFocusLost,

        /// <summary>
        /// This is sent when the application receives a low-memory notification
        /// </summary>
        OnLowMemory

    }

}