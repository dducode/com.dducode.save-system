using System;

namespace SaveSystemPackage.Exceptions {

    public class SaveSystemException : Exception {

        public SaveSystemException () { }

        public SaveSystemException (string message) : base(message) { }

        public SaveSystemException (string message, Exception innerException) : base(message, innerException) { }

    }

}