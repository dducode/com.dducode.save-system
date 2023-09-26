using System;

namespace SaveSystem.Internal.Diagnostic {

    internal readonly struct HandlerMetadata {

        internal readonly string destinationPath;
        internal readonly string caller;
        internal readonly Type handlerType;
        internal readonly Type objectsType;
        internal readonly int objectsCount;


        internal HandlerMetadata (
            string destinationPath,
            string caller,
            Type handlerType,
            Type objectsType,
            int objectsCount
        ) {
            this.destinationPath = destinationPath;
            this.caller = caller;
            this.objectsType = objectsType;
            this.objectsCount = objectsCount;
            this.handlerType = handlerType;
        }

    }

}