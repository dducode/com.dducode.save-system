namespace SaveSystem.Handlers {

    /// <summary>
    /// Defines methods for simple object handling
    /// </summary>
    public interface IObjectHandler {

        /// <summary>
        /// Call it to start objects saving
        /// </summary>
        public void Save ();


        /// <summary>
        /// Call it to start objects loading
        /// </summary>
        public HandlingResult Load ();

    }

}