﻿using SaveSystem.BinaryHandlers;

namespace SaveSystem {

    /// <summary>
    /// Implement this for simplifed handling of your objects.
    /// </summary>
    public interface IRuntimeSerializable {

        /// <summary>
        /// Writes some data to <see cref="SaveWriter"/>
        /// </summary>
        public void Serialize (SaveWriter writer);


        /// <summary>
        /// Gets <see cref="SaveReader"/> and read data from this
        /// </summary>
        public void Deserialize (SaveReader reader);

    }

}