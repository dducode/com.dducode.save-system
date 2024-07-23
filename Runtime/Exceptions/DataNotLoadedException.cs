using System;

namespace SaveSystemPackage.Exceptions {

    public class DataNotLoadedException : Exception {

        public DataNotLoadedException (string message) : base(message) { }

    }

}