using System;

namespace SaveSystemPackage.Exceptions {

    public class BinarySerializationException : Exception {

        public BinarySerializationException (string message) : base(message) { }

    }

}