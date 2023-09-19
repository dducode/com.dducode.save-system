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
        /// This is sent when the player reached a checkpoint
        /// </summary>
        SaveAtCheckpoint,

        /// <summary>
        /// This is sent when the core calls handlers to saving in internal loop
        /// </summary>
        AutoSave,

        /// <summary>
        /// This is sent when the player exit the game
        /// </summary>
        OnExit

    }

}