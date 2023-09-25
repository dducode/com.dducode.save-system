namespace SaveSystem.Handlers {

    /// <summary>
    /// Possible results enumeration of objects handling
    /// </summary>
    public enum HandlingResult {

        /// <summary>
        /// The handling succeeded
        /// </summary>
        Success,

        /// <summary>
        /// There aren't any files in the local storage at the specified path
        /// </summary>
        FileNotExists,

        /// <summary>
        /// Handling operation was canceled by external code
        /// </summary>
        CanceledOperation,

        /// <summary>
        /// An error occured while retrieving data from a remote storage
        /// </summary>
        NetworkError,

        /// <summary>
        /// Defines other errors
        /// </summary>
        UnknownError

    }

}