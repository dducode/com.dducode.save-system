namespace SaveSystem {

    /// <summary>
    /// Implement this for simplifed handling of your objects.
    /// It's easier in use than <see cref="IPersistentObject"/>
    /// </summary>
    public interface IStorable {

        /// <summary>
        /// Writes some data to <see cref="DataBuffer"/> and return this
        /// </summary>
        public DataBuffer Save ();


        /// <summary>
        /// Gets <see cref="DataBuffer"/> and read data from this
        /// </summary>
        public void Load (DataBuffer buffer);

    }

}