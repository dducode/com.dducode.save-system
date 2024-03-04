namespace SaveSystem {

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
        /// This event will be invoked during a scene loading (only when the scene loading operation starts from the Core)
        /// </summary>
        OnSceneLoad,

        /// <summary>
        /// This is sent when the player exit the game
        /// </summary>
        OnExit,

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