namespace SaveSystem.Internal {

    public static class PathPreparing {

        /// <summary>
        /// Creates new directories if they're not exists and returns full path
        /// </summary>
        public static string PrepareBeforeWriting (string path) {
            Storage.CreateFoldersIfNotExists(path);
            return Storage.GetFullPath(path);
        }

    }

}