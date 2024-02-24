namespace SaveSystem {

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
        /// The handling operation was canceled by external code
        /// </summary>
        Canceled,

        /// <summary>
        /// The handling operation failed with an internal error. Usuall you shouldn't get this
        /// </summary>
        InternalError,

    }

}