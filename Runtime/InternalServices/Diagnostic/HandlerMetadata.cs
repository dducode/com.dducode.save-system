using System;

namespace SaveSystem.InternalServices.Diagnostic {

    internal readonly struct HandlerMetadata {

        internal readonly string filePath;
        internal readonly string caller;
        internal readonly Type objectsType;
        internal readonly int objectsCount;


        internal HandlerMetadata (string filePath, string caller, Type objectsType, int objectsCount) {
            this.filePath = filePath;
            this.caller = caller;
            this.objectsType = objectsType;
            this.objectsCount = objectsCount;
        }

    }

}