using System;

namespace SaveSystem.Exceptions {

    public class DataNotLoadedException : Exception {

        public DataNotLoadedException (string message) : base(message) { }

    }

}