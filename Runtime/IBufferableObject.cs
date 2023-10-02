namespace SaveSystem {

    /// <summary>
    /// TODO: add description
    /// </summary>
    public interface IBufferableObject {

        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <returns>  </returns>
        public DataBuffer Save ();


        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="buffer">  </param>
        public void Load (DataBuffer buffer);

    }

}