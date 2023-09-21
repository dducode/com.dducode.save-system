namespace SaveSystem.Core {

    /// <summary>
    /// Defines saving methods
    /// </summary>
    public enum SaveMode {

        /// <summary>
        /// Saving will be started as usual
        /// </summary>
        Simple,
        
        /// <summary>
        /// The Core will start async saving
        /// </summary>
        Async,
        
        /// <summary>
        /// The Core will execute saving in multiple threads
        /// </summary>
        Parallel

    }

}