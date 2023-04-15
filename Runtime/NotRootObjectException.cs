using System;

namespace SaveSystem {

    public class NotRootObjectException : Exception {

        public NotRootObjectException () { }
        public NotRootObjectException (string message) : base(message) { }
        public NotRootObjectException (string message, Exception inner) : base(message, inner) { }

    }

}