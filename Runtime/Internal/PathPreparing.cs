namespace SaveSystem.Internal {

    internal static class PathPreparing {

        /// <summary>
        /// Creates new directories if they're not exists and returns full path
        /// </summary>
        internal static string PrepareBeforeWriting (string path) {
            Storage.CreateFoldersIfNotExists(path);
            return Storage.GetFullPath(path);
        }

    }

}