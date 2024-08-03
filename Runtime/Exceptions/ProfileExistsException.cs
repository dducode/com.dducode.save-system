using System;

namespace SaveSystemPackage.Exceptions {

    public class ProfileExistsException : Exception {

        public ProfileExistsException (string message) : base(message) { }

    }

}