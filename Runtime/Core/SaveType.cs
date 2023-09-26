namespace SaveSystem.Core {

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
        /// This is sent when the Core starts saving in internal loop
        /// </summary>
        AutoSave,

        /// <summary>
        /// This is sent when the player exit the game
        /// </summary>
        OnExit,
        
        /// <summary>
        /// This is sent when the application loses focus
        /// </summary>
        OnFocusChanged,
        
        /// <summary>
        /// This is sent when the application receives a low-memory notification
        /// </summary>
        OnLowMemory

    }

}